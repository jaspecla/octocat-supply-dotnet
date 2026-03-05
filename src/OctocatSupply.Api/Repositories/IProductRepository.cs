using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> FindBySupplierIdAsync(int supplierId);
    Task<IEnumerable<Product>> FindByNameAsync(string name);
}
