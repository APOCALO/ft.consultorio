using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Web.Api.RateLimiting;
using Ft.Consultorio.ServiceDefaults.Web.Api.Controllers;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.ForgotPassword;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.ExchangeAppleToken;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.ExchangeGoogleToken;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.LoginLocalUser;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.RefreshToken;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.RegisterLocalUser;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.ResetPassword;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.ResendEmailOtp;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.RevokeRefreshToken;
using Ft.Consultorio.MsAuth.Application.Auth.Commands.VerifyEmailOtp;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;

namespace Ft.Consultorio.MsAuth.Controllers.V1
{
    /// <summary>Gestiona autenticación, registro y emisión/renovación de tokens.</summary>
    [ApiController]
    [ApiVersion("1.0", Deprecated = false)]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ApiBaseController
    {
        private readonly ISender _mediator;
        private readonly HashSet<string> _allowedOrigins;

        public AuthController(ISender mediator, IConfiguration configuration)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _allowedOrigins = configuration.GetSection("Security:AllowedOrigins").Get<string[]>()?
                .Select(origin => origin.Trim())
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private bool IsOriginAllowed()
        {
            var origin = Request.Headers.Origin.ToString();
            var referer = Request.Headers.Referer.ToString();

            if (string.IsNullOrWhiteSpace(origin) && string.IsNullOrWhiteSpace(referer))
            {
                return false;
            }

            var candidate = !string.IsNullOrWhiteSpace(origin)
                ? origin
                : TryGetOriginFromReferer(referer);

            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            if (_allowedOrigins.Count == 0)
            {
                return !HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsProduction();
            }

            return _allowedOrigins.Contains(candidate);
        }

        private static string? TryGetOriginFromReferer(string referer)
        {
            if (string.IsNullOrWhiteSpace(referer)) return null;

            if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            {
                return null;
            }

            return uri.GetLeftPart(UriPartial.Authority);
        }

        /// <summary>Registro con email y password. Retorna estado pendiente de verificación por OTP.</summary>
        [HttpPost("register")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<RegisterLocalUserPendingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterLocalUserCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            command = command with
            {
                RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Verifica OTP de email y emite sesión (access + refresh token).</summary>
        [HttpPost("verify-email-otp")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<AuthTokenResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            command = command with
            {
                RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Reenvía OTP de verificación de email con protección anti abuso.</summary>
        [HttpPost("resend-email-otp")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthModerate)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<ResendEmailOtpResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendEmailOtp([FromBody] ResendEmailOtpCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            command = command with
            {
                RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Solicita recuperación de contraseña para una cuenta local.</summary>
        [HttpPost("forgot-password")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthModerate)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            command = command with
            {
                RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Consume un token de recuperación válido y restablece la contraseña.</summary>
        [HttpPost("reset-password")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<ResetPasswordResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            command = command with
            {
                RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Login con email y password.</summary>
        [HttpPost("login")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<AuthTokenResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginLocalUserCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            command = command with
            {
                RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Intercambia id_token de Google por JWT propio.</summary>
        [HttpPost("google/exchange")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<AuthTokenResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExchangeGoogleToken([FromBody] ExchangeGoogleTokenCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Intercambia id_token de Apple por JWT propio.</summary>
        [HttpPost("apple/exchange")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<AuthTokenResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExchangeAppleToken([FromBody] ExchangeAppleTokenCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Renueva el access token usando refresh token.</summary>
        [HttpPost("refresh")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthModerate)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<AuthTokenResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }

        /// <summary>Revoca el refresh token actual.</summary>
        [HttpPost("logout")]
        [EnableRateLimiting(RateLimitPolicyNames.AuthModerate)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromBody] RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            if (!IsOriginAllowed())
            {
                return Forbid();
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errors => Problem(errors)
            );
        }
    }
}
