using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public interface IDeliveryRepository : IRepository<Delivery>
{
    Task<IEnumerable<Delivery>> FindBySupplierIdAsync(int supplierId);
    Task<IEnumerable<Delivery>> FindByStatusAsync(string status);
    Task<IEnumerable<Delivery>> FindByDateRangeAsync(string startDate, string endDate);
    Task<Delivery> UpdateStatusAsync(int id, string status);
}
