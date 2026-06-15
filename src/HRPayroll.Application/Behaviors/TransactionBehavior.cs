using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not IBaseRequest)
            return await next();

        // Commands that self-manage their transactions skip the wrapping behavior
        if (request is ISelfManagesTransaction)
            return await next();

        await using var txn = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var response = await next();
            if (!response.IsError)
                await txn.CommitAsync(ct);
            else
                await txn.RollbackAsync(ct);

            return response;
        }
        catch
        {
            await txn.RollbackAsync(ct);
            throw;
        }
    }
}
