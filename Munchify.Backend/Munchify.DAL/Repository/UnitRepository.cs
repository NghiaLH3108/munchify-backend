using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class UnitRepository : GenericRepository<Unit>, IUnitRepository
{
    public UnitRepository(MunchifyDbContext context) : base(context)
    {
    }
}
