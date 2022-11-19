using aspnetserver.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace aspnetserver.Repositories
{
    public interface IItemsRepository
    {
        Task CreateAsync(Item item);
        Task DeleteAsync(Item item);
        Task<Item?> GetAsync(int warehouseId, int zoneId, int itemId);
        Task<IReadOnlyList<Item>> GetManyAsync(int warehouseId, int zoneId);
        Task UpdateAsync(Item item);
    }

    public class ItemsRepository : IItemsRepository
    {
        private readonly AspNetServerDbContext _aspNetServerDbContext;

        public ItemsRepository(AspNetServerDbContext aspNetServerDbContext)
        {
            _aspNetServerDbContext = aspNetServerDbContext;
        }

        public async Task<Item?> GetAsync(int warehouseId, int zoneId, int itemId)
        {
            return await _aspNetServerDbContext.Items.FirstOrDefaultAsync(x => x.Zone.Warehouse.Id == warehouseId && x.Zone.Id == zoneId && x.Id == itemId);
        }

        public async Task<IReadOnlyList<Item>> GetManyAsync(int warehouseId, int zoneId)
        {
            return await _aspNetServerDbContext.Items.Where(x => x.Zone.Warehouse.Id == warehouseId && x.Zone.Id == zoneId).ToListAsync();
        }

        public async Task CreateAsync(Item item)
        {
            _aspNetServerDbContext.Items.Add(item);
            await _aspNetServerDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Item item)
        {
            _aspNetServerDbContext.Items.Update(item);
            await _aspNetServerDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Item item)
        {
            _aspNetServerDbContext.Items.Remove(item);
            await _aspNetServerDbContext.SaveChangesAsync();
        }
    }
}
