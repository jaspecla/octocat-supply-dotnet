using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface IBranchRepository : IRepository<Branch>
{
    Task<IEnumerable<Branch>> FindByHeadquartersIdAsync(int headquartersId);
    Task<IEnumerable<Branch>> FindByNameAsync(string name);
}
