using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<IEnumerable<Supplier>> FindByNameAsync(string name);
}
