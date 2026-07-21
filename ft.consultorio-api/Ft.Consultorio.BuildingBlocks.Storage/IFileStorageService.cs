using ErrorOr;

namespace Ft.Consultorio.ServiceDefaults.Application.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Elimina un archivo del bucket.
        /// </summary>
        /// <param name="bucketName">Nombre del bucket.</param>
        /// <param name="objectName">Ruta/nombre del archivo dentro del bucket.</param>
        /// <returns>True si fue eliminado correctamente, o un Error en caso de fallo.</returns>
        Task<ErrorOr<bool>> DeleteFileAsync(
            string bucketName,
            string objectName);


        /// <summary>
        /// Sube un archivo desde disco.
        /// </summary>
        Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            string filePath,
            string contentType);

        /// <summary>
        /// Sube un archivo desde un Stream.
        /// </summary>
        Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            Stream fileStream,
            string contentType);

        /// <summary>
        /// Genera una URL firmada temporal para acceder a un archivo privado.
        /// </summary>
        /// <param name="bucketName">Nombre del bucket.</param>
        /// <param name="objectName">Ruta/nombre del archivo dentro del bucket.</param>
        /// <param name="expirySeconds">Tiempo en segundos que la URL será válida (por defecto 1 hora).</param>
        /// <returns>Una URL firmada temporalmente o un Error.</returns>
        Task<ErrorOr<string>> GetFileUrlAsync(
            string bucketName,
            string objectName,
            int? expirySeconds = null);

        /// <summary>
        /// Genera una URL firmada temporal para subir un archivo (PUT).
        /// </summary>
        /// <param name="bucketName">Nombre del bucket.</param>
        /// <param name="objectName">Ruta/nombre del archivo dentro del bucket.</param>
        /// <param name="expirySeconds">Tiempo en segundos que la URL será válida (por defecto 1 hora).</param>
        /// <param name="contentType">Content-Type del archivo (opcional).</param>
        /// <returns>Una URL firmada temporalmente o un Error.</returns>
        Task<ErrorOr<string>> GetUploadUrlAsync(
            string bucketName,
            string objectName,
            int? expirySeconds = null,
            string? contentType = null);

        /// <summary>
        /// Genera la URL de consumo vía CDN para un objeto del storage.
        /// - Si el bucket es <c>Public</c>, retorna una URL directa basada en <c>CdnBaseUrl</c>.
        /// - Si el bucket es <c>Private</c>, retorna una URL firmada (tipo "SAS") para ser validada por el CDN/Worker
        ///   usando parámetros <c>exp</c> y <c>sig</c>.
        /// </summary>
        /// <param name="bucketName">Nombre del bucket configurado en StorageSettings.</param>
        /// <param name="objectName">Ruta/nombre del archivo dentro del bucket (key). Ej: <c>users/123.webp</c>.</param>
        /// <param name="expirySeconds">
        /// Tiempo en segundos que la URL firmada será válida (solo aplica para buckets privados).
        /// Si es <c>null</c>, usa <c>DefaultTtlSeconds</c> del bucket.
        /// </param>
        /// <returns>URL de CDN (directa o firmada) o un Error.</returns>
        Task<ErrorOr<string>> GetCdnUrlAsync(
            string bucketName,
            string objectName,
            int? expirySeconds = null);
    }
}
