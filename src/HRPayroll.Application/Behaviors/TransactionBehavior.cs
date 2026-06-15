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

        // Commands that already manage their own transactions skip this behavior
        if (request.GetType().Name.StartsWith("Create") ||
            request.GetType().Name.StartsWith("Update") ||
            request.GetType().Name.StartsWith("Delete") ||
            request.GetType().Name.StartsWith("Assign") ||
            request.GetType().Name.StartsWith("Add") ||
            request.GetType().Name.StartsWith("Terminate") ||
            request.GetType().Name.StartsWith("Transfer"))
        {
            // These commands handle their own transactions
            return await next();
        }

        // For other commands, wrap in a transaction
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
