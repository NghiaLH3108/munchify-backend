using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Munchify.DAL.Data;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly MunchifyDbContext _context;
    
    public ICategoryRepository Categories { get; private set; }

    public UnitOfWork(MunchifyDbContext context)
    {
        _context = context;
        Categories = new CategoryRepository(_context);
    }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
