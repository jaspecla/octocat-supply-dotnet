using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> FindByBranchIdAsync(int branchId);
    Task<IEnumerable<Order>> FindByStatusAsync(string status);
    Task<IEnumerable<Order>> FindByDateRangeAsync(string startDate, string endDate);
}
