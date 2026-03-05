using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface IOrderDetailDeliveryRepository : IRepository<OrderDetailDelivery>
{
    Task<IEnumerable<OrderDetailDelivery>> FindByOrderDetailIdAsync(int orderDetailId);
    Task<IEnumerable<OrderDetailDelivery>> FindByDeliveryIdAsync(int deliveryId);
    Task<int> GetTotalQuantityByOrderDetailIdAsync(int orderDetailId);
}
