using HRPayroll.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public void Add(T entity) => DbSet.Add(entity);

    public void Update(T entity) => DbSet.Update(entity);

    public void Delete(T entity) => DbSet.Remove(entity);
}
