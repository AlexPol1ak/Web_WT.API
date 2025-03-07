﻿using Microsoft.AspNetCore.Mvc;
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
        /// <summary>
        /// получить все  телефоны.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Получить телефон по Id.
        /// </summary>
        /// <param name="id">Id телефона.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Обновить данные телефона.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhone(int id, [FromBody] Phone phone)
        {
            //DebugHelper.ShowData(id, phone.Name, phone.Model, phone.Price);

            if (id != phone.PhoneId)
            {
                //DebugHelper.ShowError("ID in the route does not match ID in the phone object.");
                return BadRequest("ID in the route does not match ID in the phone object.");
            }

            var existingPhone = await _context.Phones.FindAsync(id);
            if (existingPhone == null)
            {
                //DebugHelper.ShowError("Phone not found.");
                return NotFound("Phone not found.");
            }

            // Обновление полей существующего телефона
            existingPhone.Name = phone.Name;
            existingPhone.Model = phone.Model;
            existingPhone.Description = phone.Description;
            existingPhone.Price = phone.Price;
            existingPhone.CategoryId = phone.CategoryId;

            _context.Entry(existingPhone).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Обновляет изображение телефона
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        [HttpPost("update_image/{id}")]
        public async Task<ActionResult> UpdatePhoneImage(int id, IFormFile image)
        {
            Phone? phone = await _context.Phones.FindAsync(id);
            if (phone == null) return NotFound("Phone not found.");

            if (!string.IsNullOrEmpty(phone.Image))
            {
                string imagePath = Path.Combine(_env.WebRootPath, new Uri(phone.Image).LocalPath.TrimStart('/'));
                string normalizedImagePath = Path.GetFullPath(imagePath);
                if (System.IO.File.Exists(normalizedImagePath))
                {
                    System.IO.File.Delete(normalizedImagePath);
                }

                phone.Image = null;
            }

            var imagesPath = Path.Combine(_env.WebRootPath, "Images", "MemoryPhones");
            var randomName = Path.GetRandomFileName();
            var extension = Path.GetExtension(image.FileName);
            var fileName = Path.ChangeExtension(randomName, extension);
            var filePath = Path.Combine(imagesPath, fileName);
            using var stream = System.IO.File.OpenWrite(filePath);
            await image.CopyToAsync(stream);
            var host = "https://" + Request.Host;
            var url = $"{host}/Images/MemoryPhones/{fileName}";
            phone.Image = url;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить телефон.
        /// </summary>
        /// <param name="id">Id телефона</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhone(int id)
        {
            var phone = await _context.Phones.FindAsync(id);

            if (phone == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(phone.Image))
            {
                string imagePath = Path.Combine(_env.WebRootPath, new Uri(phone.Image).LocalPath.TrimStart('/'));
                string normalizedImagePath = Path.GetFullPath(imagePath);
                if (System.IO.File.Exists(normalizedImagePath))
                {
                    System.IO.File.Delete(normalizedImagePath);
                }

                //DebugHelper.ShowData(normalizedImagePath, phone);
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

        private bool PhoneExists(int id)
        {
            return _context.Phones.Any(e => e.PhoneId == id);
        }
    }
}
