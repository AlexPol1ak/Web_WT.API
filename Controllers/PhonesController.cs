using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Poliak_UI_WT.API.Data;
using Poliak_UI_WT.Domain.Entities;

namespace Poliak_UI_WT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhonesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PhonesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPhones()
        {
            var phones = await _context.Phones.ToListAsync();
            return Ok(phones);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetPhone(int id)
        {
            var phone = await _context.Phones.FindAsync(id);

            if (phone == null)
            {
                return NotFound();
            }

            return Ok(phone);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePhone([FromBody] Phone phone)
        {
            if (phone == null)
            {
                return BadRequest();
            }

            _context.Phones.Add(phone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhone), new { id = phone.PhoneId }, phone);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhone(int id, [FromBody] Phone phone)
        {
            if (id != phone.PhoneId)
            {
                return BadRequest("ID in the route does not match the ID in the body.");
            }

            var existingPhone = await _context.Phones.FindAsync(id);
            if (existingPhone == null)
            {
                return NotFound();
            }

            existingPhone.Name = phone.Name;
            existingPhone.Price = phone.Price;
            existingPhone.Description = phone.Description;

            _context.Entry(existingPhone).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhone(int id)
        {
            var phone = await _context.Phones.FindAsync(id);

            if (phone == null)
            {
                return NotFound();
            }

            _context.Phones.Remove(phone);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
