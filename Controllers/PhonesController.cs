using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Poliak_UI_WT.API.Data;
using Poliak_UI_WT.Domain.Entities;
using Poliak_UI_WT.Domain.Models;

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
        public async Task<ActionResult<ResponseData<ListModel<Phone>>>> GetPhones(
             string? category,
             int pageNo = 1,
             int pageSize = 3
            )
        {

            var dataQuery = _context.Phones
        .Include(d => d.Category)
        .Where(d => string.IsNullOrEmpty(category) || d.Category.NormalizedName == category);

            int totalItems = await dataQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (pageNo > totalPages && totalPages > 0)
                pageNo = totalPages;

            var phones = await dataQuery
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(phone => new Phone
                {
                    PhoneId = phone.PhoneId,
                    Name = phone.Name,
                    Model = phone.Model,
                    Description = phone.Description,
                    Image = phone.Image,
                    Price = phone.Price,
                    CategoryId = phone.CategoryId,
                    Category = phone.Category != null
                        ? new Category
                        {
                            CategoryId = phone.Category.CategoryId,
                            Name = phone.Category.Name,
                            NormalizedName = phone.Category.NormalizedName
                        }
                        : null
                })
                .ToListAsync();

            var result = new ResponseData<ListModel<Phone>>
            {
                Data = new ListModel<Phone>
                {
                    Items = phones,
                    CurrentPage = pageNo,
                    TotalPages = totalPages
                },
                Success = phones.Any(),
                Error = phones.Any() ? null : "Нет объектов в выбранной категории"
            };

            return Ok(result);
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
