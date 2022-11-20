using aspnetserver.Auth.Model;
using aspnetserver.Data.Models;
using aspnetserver.Repositories;
using aspnetserver.Data.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace aspnetserver.Controllers;

[ApiController]
[Route("api/warehouses")]

public class WarehousesController : ControllerBase
{
    private readonly IWarehousesRepository _warehousesRepository;
    private readonly IAuthorizationService _authorizationService;
    public WarehousesController(IWarehousesRepository warehousesRepository, IAuthorizationService authorizationService)
    {
        _warehousesRepository = warehousesRepository;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Authorize(Roles = WarehouseRoles.Admin + "," + WarehouseRoles.Manager)]
    public async Task<IEnumerable<WarehouseDto>> GetMany()
    {
        var warehouses = await _warehousesRepository.GetManyAsync();
        //var authorizationResult = await _authorizationService.AuthorizeAsync(User, warehouses, PolicyNames.ResourceOwner);
        //if (!authorizationResult.Succeeded)
        //{
        //    return null;
        //}
        return warehouses.Select(x => new WarehouseDto(x.Id, x.Name, x.Description, x.Address, x.CreationDate));
    }
    [HttpGet("{warehouseId}", Name = "GetWarehouse")]
    [Authorize(Roles = WarehouseRoles.Admin)]
    public async Task<ActionResult<Warehouse>> Get(int warehouseId)
    {
        var warehouse = await _warehousesRepository.GetAsync(warehouseId);

        // 404
        if (warehouse == null)
            return NotFound($"Couldn't find a warehouse with id of {warehouseId}"); ;

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, warehouse, PolicyNames.ResourceOwner);
        var x = User.IsInRole("Admin");
        if (authorizationResult.Succeeded)
        {
            var r = authorizationResult.Failure.FailedRequirements;
            var s = authorizationResult.Failure.FailureReasons;
            return Forbid();
        }

        // var warehouse1 = new Warehouse(warehouse.Id, warehouse.Name, warehouse.Description, warehouse.Address, warehouse.CreationDate);
        return new Warehouse
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Description = warehouse.Description,
            Address = warehouse.Address,
            CreationDate = warehouse.CreationDate
        };

    }

    [HttpPost]
    [Authorize(Roles = WarehouseRoles.Admin + "," + WarehouseRoles.Worker)]
    public async Task<ActionResult<Warehouse>> Create(CreateWarehouseDto createWarehouseDto)
    {

        if (createWarehouseDto.Address is not null && createWarehouseDto.Address.All(char.IsDigit) || createWarehouseDto.Address is null)
        {
            return BadRequest("You need to put valid name/description/address");
        }
        if (createWarehouseDto.Name is not null && createWarehouseDto.Name.All(char.IsDigit) || createWarehouseDto.Name is null)
        {
            return BadRequest("You need to put valid name/description/address");
        }
        if (createWarehouseDto.Description is not null && createWarehouseDto.Description.All(char.IsDigit) || createWarehouseDto.Description is null)
        {
            return BadRequest("You need to put valid name/description/address");
        }
        else
        {
            var war = new Warehouse
            {
                Name = createWarehouseDto.Name,
                Description = createWarehouseDto.Description,
                Address = createWarehouseDto.Address,
                CreationDate = DateTime.UtcNow,
                UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)

            };

            
            await _warehousesRepository.CreateAsync(war);


            //return Created("", new { id = war.Id });
            // 201
            return Created("", new WarehouseDto(war.Id, war.Name, war.Description, war.Address, DateTime.Now));

        }

        //return CreatedAtAction("GetTopic", new { topicId = topic.Id }, new TopicDto(topic.Name, topic.Description, topic.CreationDate));
    }

    // api/topics
    [HttpPut]
    [Authorize(Roles = WarehouseRoles.Admin + "," + WarehouseRoles.Worker)]
    [Route("{warehouseId}")]
    public async Task<ActionResult<WarehouseDto>> Update(int warehouseId, UpdateWarehouseDto updateWarehouseDto)
    {
        var warehouse = await _warehousesRepository.GetAsync(warehouseId);

        // 404
        if (warehouse == null)
            return NotFound($"Couldn't find a warehouse with id of {warehouseId}"); ;

        if (updateWarehouseDto.Address is not null && updateWarehouseDto.Address.All(char.IsDigit))
        {
            return BadRequest("You need to put valid description/address");
        }

        if (updateWarehouseDto.Description is not null && updateWarehouseDto.Description.All(char.IsDigit))
        {
            return BadRequest("You need to put valid description/address");
        }
        else
        {

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, warehouse, PolicyNames.ResourceOwner);
            var x = true;
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            warehouse.Description = updateWarehouseDto.Description is null ? warehouse.Description : updateWarehouseDto.Description;
            warehouse.Address = updateWarehouseDto.Address is null ? warehouse.Address : updateWarehouseDto.Address;

            await _warehousesRepository.UpdateAsync(warehouse);

            return Ok(new Warehouse
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Description = warehouse.Description,
                Address = warehouse.Address,
                CreationDate = warehouse.CreationDate
            });
        }

    }

    [HttpDelete("{warehouseId}", Name = "DeleteWarehouse")]
    [Authorize(Roles = WarehouseRoles.Admin)]
    public async Task<ActionResult> Remove(int warehouseId)
    {
        var warehouse = await _warehousesRepository.GetAsync(warehouseId);

        // 404
        if (warehouse == null)
            return NotFound($"Couldn't find a warehouse with id of {warehouseId}"); ;
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, warehouse, PolicyNames.ResourceOwner);
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }
        await _warehousesRepository.DeleteAsync(warehouse);


        // 204
        return NoContent();
    }
}
