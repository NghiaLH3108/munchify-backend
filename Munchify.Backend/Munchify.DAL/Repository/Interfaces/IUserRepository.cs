using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllStaffAsync(string? role, bool? isActive, string? searchKeyword = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<int> GetTotalStaffCountAsync(string? role, bool? isActive, string? searchKeyword = null, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ExistsPhoneAsync(string phone, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsEmailAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
