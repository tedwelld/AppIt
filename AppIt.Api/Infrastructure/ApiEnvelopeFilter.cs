using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace AppIt.Api.Infrastructure
{
    public class ApiEnvelopeFilter : IAsyncResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                WrapObjectResult(objectResult);
            }
            else if (context.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode >= 400)
            {
                var status = statusCodeResult.StatusCode;
                var pd = BuildProblemDetails(status, null, context.HttpContext);
                context.Result = new ObjectResult(ApiEnvelope.Fail(pd))
                {
                    StatusCode = status
                };
            }

            return next();
        }

        private static void WrapObjectResult(ObjectResult objectResult)
        {
            var status = objectResult.StatusCode ?? StatusCodes.Status200OK;

            if (objectResult.Value is ApiEnvelope || objectResult.Value is FileResult)
            {
                return;
            }

            if (status >= 400)
            {
                var problem = objectResult.Value as ProblemDetails
                    ?? BuildProblemDetails(status, objectResult.Value, null);

                objectResult.Value = ApiEnvelope.Fail(problem);
                return;
            }

            objectResult.Value = ApiEnvelope.Ok(objectResult.Value);
        }

        private static ProblemDetails BuildProblemDetails(int status, object? detailSource, HttpContext? httpContext)
        {
            var detail = detailSource switch
            {
                string s => s,
                ValidationProblemDetails validation => validation.Detail,
                ProblemDetails problem => problem.Detail,
                _ => null
            };

            return new ProblemDetails
            {
                Status = status,
                Title = ReasonPhrases.GetReasonPhrase(status),
                Detail = string.IsNullOrWhiteSpace(detail) ? null : detail,
                Instance = httpContext?.Request.Path
            };
        }
    }
}
