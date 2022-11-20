using aspnetserver.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace aspnetserver.Repositories
{
    public interface IWarehousesRepository
    {
        Task CreateAsync(Warehouse warehouse);
        Task DeleteAsync(Warehouse warehouse);
        Task<Warehouse?> GetAsync(int warehouseId);
        Task<IReadOnlyList<Warehouse>> GetManyAsync();
        Task UpdateAsync(Warehouse warehouse);
    }

    public class WarehousesRepository : IWarehousesRepository
    {
        private readonly WarehouseDbContext _warehouseDbContext;

        public WarehousesRepository(WarehouseDbContext warehouseDbContext)
        {
            _warehouseDbContext = warehouseDbContext;
        }

        public async Task<Warehouse?> GetAsync(int warehouseId)
        {
            return await _warehouseDbContext.Warehouses.FirstOrDefaultAsync(x => x.Id == warehouseId);
        }

        public async Task<IReadOnlyList<Warehouse>> GetManyAsync()
        {
            return await _warehouseDbContext.Warehouses.ToListAsync();
        }

        public async Task CreateAsync(Warehouse warehouse)
        {
            _warehouseDbContext.Warehouses.Add(warehouse);
            await _warehouseDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _warehouseDbContext.Warehouses.Update(warehouse);
            await _warehouseDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Warehouse warehouse)
        {
            _warehouseDbContext.Warehouses.Remove(warehouse);
            await _warehouseDbContext.SaveChangesAsync();
        }
    }
}
