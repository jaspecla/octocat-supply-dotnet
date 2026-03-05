using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    Task<IEnumerable<OrderDetail>> FindByOrderIdAsync(int orderId);
    Task<IEnumerable<OrderDetail>> FindByProductIdAsync(int productId);
    Task<decimal> GetTotalValueByOrderIdAsync(int orderId);
}
