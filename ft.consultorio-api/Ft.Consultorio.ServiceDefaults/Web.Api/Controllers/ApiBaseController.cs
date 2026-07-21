using ErrorOr;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Ft.Consultorio.ServiceDefaults.Web.Api.Constants;

namespace Ft.Consultorio.ServiceDefaults.Web.Api.Controllers
{
    /// <summary>Base para controladores API con manejo centralizado de errores.</summary>
    public class ApiBaseController : ControllerBase
    {
        protected IActionResult Problem(List<Error> errors)
        {
            if (errors.Count == 0)
            {
                return Problem();
            }

            if (errors.All(error => error.Type == ErrorType.Validation))
            {
                return ValidationProblem(errors);
            }

            HttpContext.Items[HttpContextItemKeys.Errors] = errors;

            return Problem(errors[0]);
        }

        private IActionResult Problem(Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError,
            };

            var title = statusCode >= 500
                ? "Server Error"
                : error.Description;

            return Problem(statusCode: statusCode, title: title);
        }

        private IActionResult ValidationProblem(List<Error> errors)
        {
            var modelStateDictionary = new ModelStateDictionary();

            foreach (var error in errors)
            {
                modelStateDictionary.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelStateDictionary);
        }
    }
}
