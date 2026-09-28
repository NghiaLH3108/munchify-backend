using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Munchify.DAL.Repository.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IStockMovementRepository StockMovements { get; }
    IGenericRepository<Munchify.DAL.Entities.Product> Products { get; }
    
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

