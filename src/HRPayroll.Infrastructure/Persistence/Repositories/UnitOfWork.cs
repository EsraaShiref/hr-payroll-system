using HRPayroll.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);

    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct = default)
    {
        var txn = await _dbContext.Database.BeginTransactionAsync(ct);
        return new TransactionScope(txn);
    }

    private sealed class TransactionScope(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        : ITransactionScope
    {
        public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default) => transaction.RollbackAsync(ct);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
