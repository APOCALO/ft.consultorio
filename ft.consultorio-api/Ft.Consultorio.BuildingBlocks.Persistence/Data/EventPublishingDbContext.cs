using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // <-- Usa GetService<T>()
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.BuildingBlocks.Persistence.Outbox;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Data
{
    /// <summary>
    /// DbContext base compartido por los microservicios: aplica las configuraciones del
    /// assembly del contexto derivado y persiste los DomainEvents como Transactional
    /// Outbox en la misma transacción que los cambios de negocio.
    ///
    /// Despacho: tras el commit se intenta publicar inmediatamente (misma latencia que
    /// el modelo anterior); lo que falle o quede pendiente por un crash lo recoge el
    /// barrido de OutboxProcessor. Entrega at-least-once: los handlers deben ser
    /// idempotentes.
    /// </summary>
    public abstract class EventPublishingDbContext : DbContext, IApplicationDbContext, IUnitOfWork
    {
        // Único ctor (solo con opciones), compatible con AddDbContextPool en los derivados.
        protected EventPublishingDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            // Aplica las IEntityTypeConfiguration del assembly del contexto concreto.
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var pendingEvents = DequeueAllDomainEvents();

            if (pendingEvents.Count == 0)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            // Outbox: los eventos se persisten en el MISMO SaveChanges que los cambios
            // de negocio (una llamada a SaveChanges es atómica), así que no se pierden
            // aunque el proceso muera antes de publicarlos.
            var messages = pendingEvents.Select(OutboxMessage.From).ToList();
            Set<OutboxMessage>().AddRange(messages);

            var result = await base.SaveChangesAsync(cancellationToken);

            // Despacho inmediato best-effort. Los fallos quedan registrados en el
            // mensaje y los reintenta OutboxProcessor.
            await DispatchAsync(messages, cancellationToken);

            return result;
        }

        private async Task DispatchAsync(List<OutboxMessage> messages, CancellationToken cancellationToken)
        {
            // OJO: this.GetService<T>() resuelve primero contra el contenedor INTERNO de EF
            // (que tiene su propio IServiceScopeFactory sin MediatR). Hay que ir explícitamente
            // al provider de la APLICACIÓN. Y se necesita un scope propio porque con
            // AddDbContextPool el ApplicationServiceProvider es el raíz, y resolver handlers
            // con dependencias scoped desde el raíz falla con ValidateScopes (Development).
            var applicationProvider = this.GetService<IDbContextOptions>()
                .FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
            var scopeFactory = applicationProvider?.GetService<IServiceScopeFactory>();

            if (scopeFactory is null)
            {
                // Sin DI de aplicación (design-time, etc.): los mensajes quedan pendientes
                // y los recoge el OutboxProcessor cuando haya host.
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var publisher = scope.ServiceProvider.GetService<IPublisher>() ?? new NoopPublisher();
            var logger = scope.ServiceProvider.GetService<ILogger<EventPublishingDbContext>>()
                ?? NullLogger<EventPublishingDbContext>.Instance;

            foreach (var message in messages)
            {
                try
                {
                    var domainEvent = message.Deserialize();
                    if (domainEvent is null)
                    {
                        message.MarkFailed(new InvalidOperationException($"Cannot resolve event type '{message.Type}'."));
                        continue;
                    }

                    await publisher.Publish(domainEvent, cancellationToken);
                    message.MarkProcessed();
                }
                catch (Exception ex)
                {
                    message.MarkFailed(ex);
                    logger.LogError(ex, "Error publishing DomainEvent {EventType}; it will be retried by the outbox sweep.",
                        message.Type);
                }
            }

            // Persiste las marcas de procesado y, si los handlers generaron nuevos
            // eventos sobre agregados trackeados, la recursión de SaveChangesAsync
            // los escribe y despacha también.
            await SaveChangesAsync(cancellationToken);
        }

        private List<DomainEvent> DequeueAllDomainEvents()
        {
            var aggregates = ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Count > 0)
                .ToList();

            var events = aggregates.SelectMany(e => e.DomainEvents).ToList();

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            return events;
        }

        // Publisher no-op para escenarios donde no esté registrado (design-time, etc.).
        private sealed class NoopPublisher : IPublisher
        {
            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification => Task.CompletedTask;
        }
    }
}
