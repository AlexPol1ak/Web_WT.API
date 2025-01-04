using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Poliak_UI_WT.API.Data;
using Poliak_UI_WT.Domain.Entities;
using Poliak_UI_WT.Domain.Models;
using Poliak_UI_WT.Domain.Utils;

namespace Poliak_UI_WT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhonesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PhonesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        /// <summary>
        /// Добавить новый телефон
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePhone([FromBody] Phone phone)
        {
            if (phone == null)
            {
                return BadRequest();
            }
            DebugHelper.ShowData(phone);
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

        /// <summary>
        /// Добавить изображение к телефону.
        /// </summary>
        /// <param name="id"> ID телефона</param>
        /// <param name="image"> Файл изображения.</param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public async Task<IActionResult> SaveImage(int id, IFormFile image)
        {
            // Найти объект по Id
            var phone = await _context.Phones.FindAsync(id);
            if (phone == null)
            {
                return NotFound();
            }
            // Путь к папке wwwroot/Images
            var imagesPath = Path.Combine(_env.WebRootPath, "Images", "MemoryPhones");
            // получить случайное имя файла
            var randomName = Path.GetRandomFileName();
            // получить расширение в исходном файле
            var extension = Path.GetExtension(image.FileName);
            // задать в новом имени расширение как в исходном файле
            var fileName = Path.ChangeExtension(randomName, extension);
            // полный путь к файлу
            var filePath = Path.Combine(imagesPath, fileName);
            // создать файл и открыть поток для записи
            using var stream = System.IO.File.OpenWrite(filePath);
            // скопировать файл в поток
            await image.CopyToAsync(stream);
            // получить Url хоста
            var host = "https://" + Request.Host;
            // Url файла изображения
            var url = $"{host}/Images/MemoryPhones/{fileName}";
            // Сохранить url файла в объекте
            phone.Image = url;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
