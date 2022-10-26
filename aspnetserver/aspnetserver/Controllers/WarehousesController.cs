
using aspnetserver.Data.Models;
using aspnetserver.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace aspnetserver.Controllers
{
    [ApiController]
    [Route("api/warehouses")]

    public class WarehousesController : ControllerBase
    {
        private readonly IWarehousesRepository _warehousesRepository;
        //private readonly IMapper _mapper;


        public WarehousesController(IWarehousesRepository warehousesRepository)
        {
            _warehousesRepository = warehousesRepository;
        }


        [HttpGet]
        public async Task<IEnumerable<WarehouseDto>> GetMany()
        {
            var warehouses = await _warehousesRepository.GetManyAsync();
            return warehouses.Select(x => new WarehouseDto(x.Id, x.Name, x.Description, x.Address, x.CreationDate));
        }
        [HttpGet("{warehouseId}", Name = "GetWarehouse")]
        public async Task<ActionResult<WarehouseDto>> Get(int warehouseId)
        {
            var warehouse = await _warehousesRepository.GetAsync(warehouseId);

            // 404
            if (warehouse == null)
                return NotFound($"Couldn't find a warehouse with id of {warehouseId}"); ;


            // var warehouse1 = new Warehouse(warehouse.Id, warehouse.Name, warehouse.Description, warehouse.Address, warehouse.CreationDate);
            var dto = new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Description, warehouse.Address, warehouse.CreationDate);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseDto createWarehouseDto)
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
                { Name = createWarehouseDto.Name, Description = createWarehouseDto.Description, Address = createWarehouseDto.Address, CreationDate = DateTime.UtcNow };

                await _warehousesRepository.CreateAsync(war);


                //return Created("", new { id = war.Id });
                // 201
                return Created("", new WarehouseDto(war.Id, war.Name, war.Description, war.Address, DateTime.Now));

            }

            //return CreatedAtAction("GetTopic", new { topicId = topic.Id }, new TopicDto(topic.Name, topic.Description, topic.CreationDate));
        }
        // api/topics
        [HttpPut]
        [Route("{warehouseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                warehouse.Description = updateWarehouseDto.Description is null ? warehouse.Description : updateWarehouseDto.Description;
                warehouse.Address = updateWarehouseDto.Address is null ? warehouse.Address : updateWarehouseDto.Address;

                await _warehousesRepository.UpdateAsync(warehouse);

                return Ok(new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Description, warehouse.Address, warehouse.CreationDate));
            }

        }

        [HttpDelete("{warehouseId}", Name = "DeleteWarehouse")]
        public async Task<ActionResult> Remove(int warehouseId)
        {
            var warehouse = await _warehousesRepository.GetAsync(warehouseId);

            // 404
            if (warehouse == null)
                return NotFound($"Couldn't find a warehouse with id of {warehouseId}"); ;

            await _warehousesRepository.DeleteAsync(warehouse);


            // 204
            return NoContent();
        }
    }   
}
