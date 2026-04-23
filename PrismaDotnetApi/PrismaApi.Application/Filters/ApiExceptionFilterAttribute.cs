using System.Text;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PrismaApi.Application.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    public ApiExceptionFilterAttribute() =>
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(BadHttpRequestException), HandleBadHttpRequestException },
            { typeof(NullReferenceException), HandleNullReferenceException },
            { typeof(ArgumentOutOfRangeException), HandleArgumentOutOfRangeException },
            { typeof(ArgumentException), HandleArgumentException },
            { typeof(DbUpdateException), HandleDbUpdateException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(TaskCanceledException), HandleTaskCanceledException },
            { typeof(SqlException), HandleSqlException },
            { typeof(CannotInsertNullException), HandleInsertNullSqlException },
            { typeof(UniqueConstraintException), HandleUniqueConstraintSqlException }
        };

    private static void HandleBadHttpRequestException(ExceptionContext context)
    {
        var exception = (BadHttpRequestException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Status = 400,
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new ObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleUniqueConstraintSqlException(ExceptionContext context)
    {
        var exception = (UniqueConstraintException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5",
            Status = 400,
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new ObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleInsertNullSqlException(ExceptionContext context)
    {
        var exception = (CannotInsertNullException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5",
            Status = 400,
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new ObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleNullReferenceException(ExceptionContext context)
    {
        var exception = (NullReferenceException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5",
            Status = 400,
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new ObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleSqlException(ExceptionContext context)
    {
        var exception = (SqlException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6",
            Title = $"SQLServer Number: {exception.Number} occurred",
            Detail = GetSqlErrors(exception)
        };

        // https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors
        switch (exception.Number)
        {
            case -2:
            case 2:
                details.Title = "A timeout occurred while processing your request.";
                context.Result = new ObjectResult(details) { StatusCode = 503 };
                break;
            case 156:
                details.Title = "Invalid SQL detected in request.";
                context.Result = new ObjectResult(details) { StatusCode = 500 };
                break;
            case 1205:
                details.Title = "A deadlock occurred while processing your request.";
                context.Result = new ObjectResult(details) { StatusCode = 503 };
                break;
            case 547:
            case 548:
            case 2601:
            case 2627:
                details.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
                details.Title = "Database constraint violation occurred.";
                context.Result = new BadRequestObjectResult(details);
                break;
            default:
                context.Result = new StatusCodeResult(500);
                break;
        }

        context.ExceptionHandled = true;
    }


    private static string GetSqlErrors(SqlException exception)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < exception.Errors.Count; i++)
        {
            sb.AppendLine("Index #" + i + "Error: " + exception.Errors[i]);
        }

        return sb.ToString();
    }

    private static void HandleTaskCanceledException(ExceptionContext context)
    {
        var exception = (TaskCanceledException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5",
            Status = 499,
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new ObjectResult(details);

        context.ExceptionHandled = true;
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);

        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        var type = context.Exception.GetType();
        if (_exceptionHandlers.TryGetValue(type, out var handler))
        {
            handler.Invoke(context);
            return;
        }

        if (!context.ModelState.IsValid)
        {
            HandleInvalidModelStateException(context);
        }
    }

    private static void HandleDbUpdateException(ExceptionContext context)
    {
        var exception = (DbUpdateException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }


    private static void HandleArgumentOutOfRangeException(ExceptionContext context)
    {
        var exception = (ArgumentOutOfRangeException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleArgumentException(ExceptionContext context)
    {
        var exception = (ArgumentException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Detail = exception.GetInnermostExceptionMessage()
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleInvalidModelStateException(ExceptionContext context)
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleUnauthorizedAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

        context.ExceptionHandled = true;
    }
}

public static class ExceptionExtensions
{
    public static string GetInnermostExceptionMessage(this Exception ex)
    {
        if (ex == null)
        {
            throw new ArgumentNullException(nameof(ex), "Exception cannot be null.");
        }

        var innerException = ex;
        while (innerException.InnerException != null)
        {
            innerException = innerException.InnerException;
        }

        return innerException.Message;
    }
}
