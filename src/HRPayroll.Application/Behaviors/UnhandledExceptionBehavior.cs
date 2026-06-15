using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRPayroll.Application.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {RequestName}", typeof(TRequest).Name);

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(ErrorOr<>))
            {
                var error = Error.Unexpected("InternalServerError", "An unexpected error occurred.");
                var responseType = typeof(TResponse).GetGenericArguments()[0];
                var errorOrType = typeof(ErrorOr<>).MakeGenericType(responseType);
                var result = Activator.CreateInstance(errorOrType, new object[] { error });
                return (TResponse)result!;
            }

            throw;
        }
    }
}
