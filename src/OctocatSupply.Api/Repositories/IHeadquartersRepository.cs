using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface IHeadquartersRepository : IRepository<Headquarters>
{
    Task<IEnumerable<Headquarters>> FindByNameAsync(string name);
}
