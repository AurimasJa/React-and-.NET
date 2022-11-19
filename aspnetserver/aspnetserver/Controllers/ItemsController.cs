using aspnetserver.Data.Dtos;
using aspnetserver.Data.Models;
using aspnetserver.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace aspnetserver.Controllers
{
    public class ItemsController : Controller
    {
        [ApiController]
        [Route("api/warehouses/{warehouseId}/zones/{zoneId}/items")]
        public class ItemsController : ControllerBase
        {
            private readonly IZonesRepository _zoneRepository;
            private readonly IWarehousesRepository _warehousesRepository;
            private readonly IAuthorizationService _authorizationService;
            private readonly IItemsRepository _itemsRepository;

            public ItemsController(IZonesRepository zoneRepository, IWarehousesRepository warehousesRepository, IAuthorizationService authorizationService, IItemsRepository itemsRepository)
            {
                _zoneRepository = zoneRepository;
                _warehousesRepository = warehousesRepository;
                _authorizationService = authorizationService;
                _itemsRepository = itemsRepository;
            }

            [HttpGet]
            [Route("{itemId}")]
            [Authorize(Roles = WarehouseRoles.Admin + "," + WarehouseRoles.Manager + "," + WarehouseRoles.Worker)]
            public async Task<ActionResult<ItemDto>> GetOne(int warehouseId, int zoneId, int itemId)
            {
                var warehouse = await _warehousesRepository.GetAsync(warehouseId);
                if (warehouse == null)
                {
                    return NotFound($"Couldn't find a warehouse with id of {warehouseId}");
                }
                var zone = await _zoneRepository.GetAsync(warehouseId, zoneId);
                if (zone == null)
                {
                    return NotFound($"Zone {zoneId}id does not exist");
                }
                var item = await _itemsRepository.GetAsync(warehouseId, zoneId, itemId);
                if (item == null)
                {
                    return NotFound($"Item {itemId}id does not exist");
                }
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, item, PolicyNames.ResourceOwner);
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }
                var dto = new ItemDto(item.Id, item.Name, item.Description);
                return Ok(dto);
            }
            [HttpGet]
            [Authorize(Roles = WarehouseRoles.Admin + "," + WarehouseRoles.Manager + "," + WarehouseRoles.Worker)]
            public async Task<IEnumerable<ItemDto>> GetAllAsync(int warehouseId, int zoneId)
            {
                var items = await _itemsRepository.GetManyAsync(warehouseId, zoneId);
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, items, PolicyNames.ResourceOwner);
                if (!authorizationResult.Succeeded)
                {
                    return null;
                }
                return items.Select(x => new ItemDto(x.Id, x.Name, x.Description));
            }

            [HttpPost]
            [Authorize(Roles = WarehouseRoles.Admin + ", " + WarehouseRoles.Manager)]
            public async Task<ActionResult<ItemDto>> Create(int warehouseId, int zoneId, ItemDto itemDto)
            {
                var warehouse = await _warehousesRepository.GetAsync(warehouseId);
                if (warehouse == null)
                {
                    return NotFound($"Couldn't find a warehouse with id of {warehouseId}");
                }
                var zones = await _zoneRepository.GetManyAsync(warehouseId);
                var realZone = zones.FirstOrDefault();
                if (realZone == null)
                {
                    return NotFound($"Zone {realZone} id does not exist");
                }
                var zone = await _zoneRepository.GetAsync(warehouseId, zoneId);
                if (zone == null)
                {
                    return NotFound($"Zone {zoneId}id does not exist");
                }

                if (itemDto.Name is not null && itemDto.Name.All(char.IsDigit) || itemDto.Name is null)
                {
                    return BadRequest("You need to put valid name/description");
                }
                if (itemDto.Description is not null && itemDto.Description.All(char.IsDigit) || itemDto.Description is null)
                {
                    return BadRequest("You need to put valid name/description");
                }
                else
                {
                    var item = new Item
                    { Name = itemDto.Name,
                    Description = itemDto.Description};
                    item.UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                    item.ZoneId = zoneId;
                    item.Zone = realZone;
                    await _itemsRepository.CreateAsync(item);
                    var dto = new ItemDto(item.Id, item.Name, item.Description);
                    return Created($"/api/topics/{warehouseId}/posts/{zoneId}/items/{item.Id}", dto);
                }
            }
            [HttpPut("{itemId}")]
            [Authorize(Roles = WarehouseRoles.Admin + "," + WarehouseRoles.Worker)]
            public async Task<ActionResult<ItemDto>> Update(int warehouseId, int zoneId, int itemId, UpdateItemDto itemDto)
            {
                var warehouse = await _warehousesRepository.GetAsync(warehouseId);
                if (warehouse == null)
                {
                    return NotFound($"Couldn't find a warehouse with id of {warehouseId}");
                }
                var zone = await _zoneRepository.GetAsync(warehouseId, zoneId);
                if (zone == null)
                {
                    return NotFound($"Zone {zoneId}id does not exist");
                }
                var oldItem = await _itemsRepository.GetAsync(warehouseId, zoneId, itemId);
                if (oldItem == null)
                {
                    return NotFound($"Item {itemId}id does not exist");
                }

                if (itemDto.Name is not null && itemDto.Name.All(char.IsDigit))
                {
                    return BadRequest("You need to put valid name/description");
                }
                if (itemDto.Description is not null && itemDto.Description.All(char.IsDigit))
                {
                    return BadRequest("You need to put valid name/description");
                }

                else
                {
                    var authorizationResult = await _authorizationService.AuthorizeAsync(User, oldItem, PolicyNames.ResourceOwner);
                    if (!authorizationResult.Succeeded)
                    {
                        return Forbid();
                    }
                    //_mapper.Map(itemDto, oldItem);
                    oldItem.ZoneId = zoneId;
                    oldItem.Name = itemDto.Name is null ? oldItem.Name : itemDto.Name;
                    oldItem.Description = itemDto.Description is null ? oldItem.Description : itemDto.Description;
                    oldItem.ZoneId = itemDto.ZoneIdAsked != oldItem.ZoneId ? oldItem.ZoneId : itemDto.ZoneIdAsked;

                    var zoneWhere = await _zoneRepository.GetAsync(warehouseId, itemDto.ZoneIdAsked);
                    if (zoneWhere == null)
                    {
                        return NotFound($"Zone {oldItem.ZoneId}id does not exist!!!");
                    }
                    oldItem.Zone = zoneWhere;
                    await _itemsRepository.UpdateAsync(oldItem);

                    var dto = new ItemDto(item.Id, item.Name, item.Description);
                    return Ok(dto);

                }
            }
            [HttpDelete("{itemId}")]
            [Authorize(Roles = WarehouseRoles.Admin + "," + WarehouseRoles.Worker)]
            public async Task<ActionResult> Remove(int warehouseId, int zoneId, int itemId)
            {
                var warehouse = await _warehousesRepository.GetAsync(warehouseId);
                if (warehouse == null)
                {
                    return NotFound($"Couldn't find a warehouse with id of {warehouseId}");
                }
                var zone = await _zoneRepository.GetAsync(warehouseId, zoneId);
                if (zone == null)
                {
                    return NotFound($"Zone {zoneId}id does not exist");
                }
                var item = await _itemsRepository.GetAsync(warehouseId, zoneId, itemId);
                if (item == null)
                {
                    return NotFound($"Item {itemId}id does not exist");
                }
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, item, PolicyNames.ResourceOwner);
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }
                await _itemsRepository.DeleteAsync(item);

                // 204
                return NoContent();
            }
        }
    }
}
