using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Munchify.DAL.Repository.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IUnitRepository Units { get; }
    IStockMovementRepository StockMovements { get; }
    IProductRepository Products { get; }
    IDamagedExpiredRecordRepository DamagedExpiredRecords { get; }
    IStockImportRepository StockImports { get; }
    IPromotionRepository Promotions { get; }
    
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

