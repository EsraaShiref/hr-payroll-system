using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublisher _publisher;

    public UnitOfWork(ApplicationDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var domainEventEntities = _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToArray();

        var result = await _dbContext.SaveChangesAsync(ct);

        foreach (var entity in domainEventEntities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                await _publisher.Publish(domainEvent, ct);
            }
            entity.ClearDomainEvents();
        }

        return result;
    }

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
