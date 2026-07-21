using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;

namespace Ft.Consultorio.BuildingBlocks.Persistence.Outbox
{
    /// <summary>
    /// Barrido de respaldo del Transactional Outbox. El camino feliz despacha los
    /// eventos inmediatamente tras el commit (ver EventPublishingDbContext); este
    /// procesador recoge lo que quedó pendiente por un crash o un fallo transitorio.
    /// Entrega at-least-once: los handlers deben ser idempotentes.
    /// </summary>
    public sealed class OutboxProcessor<TContext> : BackgroundService
        where TContext : EventPublishingDbContext
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

        // Solo barre mensajes con cierta antigüedad para no competir con el
        // despacho inmediato que ocurre dentro de la petición.
        private static readonly TimeSpan MinAge = TimeSpan.FromSeconds(15);

        private const int BatchSize = 20;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessor<TContext>> _logger;

        public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor<TContext>> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox sweep failed; retrying on next interval.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task ProcessBatchAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            var cutoff = DateTime.UtcNow - MinAge;

            var pending = await dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedOnUtc == null
                    && m.AttemptCount < OutboxMessage.MaxAttempts
                    && m.OccurredOnUtc <= cutoff)
                .OrderBy(m => m.OccurredOnUtc)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (pending.Count == 0)
            {
                return;
            }

            foreach (var message in pending)
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
                    _logger.LogWarning(ex, "Failed to publish outbox message {MessageId} ({EventType}), attempt {Attempt}.",
                        message.Id, message.Type, message.AttemptCount);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public static class OutboxProcessorExtensions
    {
        /// <summary>Registra el barrido de respaldo del outbox para el DbContext del servicio.</summary>
        public static IServiceCollection AddOutboxProcessor<TContext>(this IServiceCollection services)
            where TContext : EventPublishingDbContext
        {
            services.AddHostedService<OutboxProcessor<TContext>>();
            return services;
        }
    }
}
