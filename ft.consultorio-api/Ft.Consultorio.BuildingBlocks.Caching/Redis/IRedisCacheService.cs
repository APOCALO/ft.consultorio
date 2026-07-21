namespace Ft.Consultorio.ServiceDefaults.Application.Interfaces
{
    public interface IRedisCacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        // Método para obtener múltiples valores en un solo roundtrip.
        Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys);

        /// <summary>
        /// Atomically increments a counter in Redis (INCR command).
        /// Returns the new value after the increment. If the key does not exist,
        /// it is initialized to 0 and incremented to 1.
        /// </summary>
        /// <remarks>
        /// This is the ONLY safe primitive for cache version-bumping because INCR is
        /// atomic by Redis specification. Using GET + SET (or timestamps) creates
        /// race conditions across concurrent reads, writers, and pods.
        /// </remarks>
        Task<long> IncrementAsync(string key, TimeSpan? expiry = null);
    }
}
