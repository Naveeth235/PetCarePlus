using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Inventory;

namespace PetCare.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query is required.");
            var items = await _service.GetAllAsync();
            var results = items.Where(i =>
                (!string.IsNullOrEmpty(i.Name) && i.Name.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(i.Category) && i.Category.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(i.Supplier) && i.Supplier.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
            ).ToList();
            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                // Return 400 BadRequest with duplicate message
                return BadRequest(new { duplicate = true, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var success = await _service.UpdateAsync(id, dto);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // Return 400 BadRequest with duplicate message
                return BadRequest(new { duplicate = true, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
