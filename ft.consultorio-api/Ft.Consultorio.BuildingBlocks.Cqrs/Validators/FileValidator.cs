using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Ft.Consultorio.ServiceDefaults.Application.Validators
{
    public class FileValidator : AbstractValidator<IFormFile>
    {
        // 10 MB.
        public const long MaxBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes =
            new(new[] { "image/png", "image/jpeg", "image/webp" }, StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> AllowedExtensions =
            new(new[] { ".png", ".jpg", ".jpeg", ".webp" }, StringComparer.OrdinalIgnoreCase);

        // Mapa simple de extensiones -> tipos MIME válidos.
        private static readonly Dictionary<string, string[]> ExtToContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".png"] = new[] { "image/png" },
                [".jpg"] = new[] { "image/jpeg" },
                [".jpeg"] = new[] { "image/jpeg" },
                [".webp"] = new[] { "image/webp" },
            };

        public FileValidator()
        {
            // Debe venir un archivo.
            RuleFor(file => file)
                .NotNull()
                .WithMessage("A file is required.");

            // Reglas que solo aplican si hay archivo.
            When(file => file is not null, () =>
            {
                // No vacío y no mayor a 10 MB.
                RuleFor(file => file!.Length)
                    .GreaterThan(0).WithMessage("The file is empty.")
                    .LessThanOrEqualTo(MaxBytes).WithMessage($"The file must not exceed {MaxBytes / (1024 * 1024)}MB.");

                // Content-Type permitido (normalizando a minúsculas).
                RuleFor(file => (file!.ContentType ?? string.Empty).ToLowerInvariant())
                    .Must(ct => AllowedContentTypes.Contains(ct))
                    .WithMessage("Only PNG, WEBP or JPEG files are allowed.");

                // Extensión permitida.
                RuleFor(file => Path.GetExtension(file!.FileName).ToLowerInvariant())
                    .Must(ext => AllowedExtensions.Contains(ext))
                    .WithMessage("File extension must be .png, .jpg, .jpeg or .webp.");

                // Coherencia extensión <-> content-type (opcional, pero recomendable).
                RuleFor(file => file)
                    .Must(file =>
                    {
                        var ext = Path.GetExtension(file!.FileName)?.ToLowerInvariant() ?? string.Empty;
                        var ct = (file.ContentType ?? string.Empty).ToLowerInvariant();

                        return ExtToContentTypes.TryGetValue(ext, out var validCts) && validCts.Contains(ct);
                    })
                    .WithMessage("The file extension does not match the content type.");

                // Firma binaria (magic bytes) para evitar confiar solo en metadatos del cliente.
                RuleFor(file => file)
                    .Must(file => MatchesExpectedSignature(file!))
                    .WithMessage("The file content does not match the declared image type.");
            });
        }

        private static bool MatchesExpectedSignature(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

            try
            {
                using var stream = file.OpenReadStream();
                var header = new byte[12];
                var read = stream.Read(header, 0, header.Length);

                if (read < 3)
                {
                    return false;
                }

                if (ext is ".jpg" or ".jpeg")
                {
                    return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
                }

                if (ext == ".png")
                {
                    if (read < 8)
                    {
                        return false;
                    }

                    return header[0] == 0x89 &&
                           header[1] == 0x50 &&
                           header[2] == 0x4E &&
                           header[3] == 0x47 &&
                           header[4] == 0x0D &&
                           header[5] == 0x0A &&
                           header[6] == 0x1A &&
                           header[7] == 0x0A;
                }

                if (ext == ".webp")
                {
                    if (read < 12)
                    {
                        return false;
                    }

                    return header[0] == 0x52 && // R
                           header[1] == 0x49 && // I
                           header[2] == 0x46 && // F
                           header[3] == 0x46 && // F
                           header[8] == 0x57 && // W
                           header[9] == 0x45 && // E
                           header[10] == 0x42 && // B
                           header[11] == 0x50; // P
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
