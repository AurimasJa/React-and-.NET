using aspnetserver.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace aspnetserver.Repositories
{
    public interface IZonesRepository
    {
        Task CreateAsync(Zone zone);
        Task DeleteAsync(Zone zone);
        Task<Zone?> GetAsync(int warehouseId, int zoneId);
        Task<IReadOnlyList<Zone>> GetManyAsync(int warehouseId);
        Task UpdateAsync(Zone zone);
    }

    public class ZonesRepository : IZonesRepository
    {
        private readonly WarehouseDbContext _aspNetServerDbContext;

        public ZonesRepository(WarehouseDbContext aspNetServerDbContext)
        {
            _aspNetServerDbContext = aspNetServerDbContext;
        }

        public async Task<Zone?> GetAsync(int warehouseId, int zoneId)
        {
            return await _aspNetServerDbContext.Zones.FirstOrDefaultAsync(x => x.Warehouse.Id == warehouseId && x.Id == zoneId);
        }

        public async Task<IReadOnlyList<Zone>> GetManyAsync(int warehouseId)
        {
            return await _aspNetServerDbContext.Zones.Where(x => x.Warehouse.Id == warehouseId).ToListAsync();
        }

        public async Task CreateAsync(Zone zone)
        {
            _aspNetServerDbContext.Zones.Add(zone);
            await _aspNetServerDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Zone zone)
        {
            _aspNetServerDbContext.Zones.Update(zone);
            await _aspNetServerDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Zone zone)
        {
            _aspNetServerDbContext.Zones.Remove(zone);
            await _aspNetServerDbContext.SaveChangesAsync();
        }
    }
}
