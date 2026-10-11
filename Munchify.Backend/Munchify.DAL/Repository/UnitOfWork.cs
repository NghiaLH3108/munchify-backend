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
    public IUnitRepository Units { get; private set; }
    public IStockMovementRepository StockMovements { get; private set; }
    public IProductRepository Products { get; private set; }
    public IDamagedExpiredRecordRepository DamagedExpiredRecords { get; private set; }
    public IStockImportRepository StockImports { get; private set; }
    public IPromotionRepository Promotions { get; private set; }

    public UnitOfWork(MunchifyDbContext context)
    {
        _context = context;
        Categories = new CategoryRepository(_context);
        Units = new UnitRepository(_context);
        StockMovements = new StockMovementRepository(_context);
        Products = new ProductRepository(_context);
        DamagedExpiredRecords = new DamagedExpiredRecordRepository(_context);
        StockImports = new StockImportRepository(_context);
        Promotions = new PromotionRepository(_context);
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
