using BookingWepApp.Data;
using BookingWepApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingWepApp.Controllers
{
    //Api контроллер для Swagger'а
    [Route("api/[controller]")]
    //только так Swagger поймет, что данный контроллер содержит методы API
    [ApiController]
    public class HotelsApiController : Controller
    {
        //создаем контекст
        private readonly ApplicationDbContext _context;

        //инициализируем контроллер
        public HotelsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        //метод GET
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hotel>>> Get()
        {
            //возвращает список отелей
            return await _context.Hotels.ToListAsync();
        }

        //метод GET/id
        [HttpGet("{id}")]
        public async Task<ActionResult<Hotel>> Get(int id)
        {
            //ищем отель по ID
            var item = await _context.Hotels.FindAsync(id);
            //если отель не найден
            if (item == null)
            {
                //ошибка 404
                return NotFound();
            }
            //иначе, возвращаем найденный отель
            return item;
        }

        //метод POST
        [HttpPost]
        public async Task<ActionResult<Hotel>> Post(Hotel item)
        {
            //добавляем отель в БД
            _context.Hotels.Add(item);
            //сохраняем
            await _context.SaveChangesAsync();
            //возвращаем статус 201
            return CreatedAtAction(
                nameof(Get),
                new { id = item.Id },
                item);
        }

        //метод PUT/id
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, Hotel item)
        {
            //если ID не совпадаюи
            if (id != item.Id)
            {
                //ошибка
                return BadRequest();
            }
            //находим отель по ID
            var foundItem = await _context.Hotels.FindAsync(id);
            //если он не найден
            if (foundItem == null)
            {
                //ошибка 404
                return NotFound();
            }
            //иначе, применяем новые данные для отеля
            foundItem.Name = item.Name;
            foundItem.Stars = item.Stars;
            foundItem.Address = item.Address;
            foundItem.City = item.City;
            foundItem.Country = item.Country;
            foundItem.Description = item.Description;
            foundItem.DistanceFromCenter = item.DistanceFromCenter;
            foundItem.ZipCode = item.ZipCode;
            //обрабатываем исключения
            try
            {
                //сохраняем данные
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!ItemExists(id))
            {
                //если возникла ошибка
                return NotFound();
            }
            //иначе, статус 204
            return NoContent();
        }

        //метод DELETE/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //ищем отель по ID
            var item = await _context.Hotels.FindAsync(id);
            //если отель не найден
            if (item == null)
            {
                //ошибка 404
                return NotFound();
            }
            //иначе, удаляем отель из БД
            _context.Hotels.Remove(item);
            //сохраняем
            await _context.SaveChangesAsync();
            //статус 204
            return NoContent();
        }
        
        //проверяем, существует ли отель
        private bool ItemExists(int id)
        {
            //ищем в БД отели по ID
            //если есть хотя бы один, то возвращаем True,
            //иначе - False
            return _context.Hotels.Any(e => e.Id == id);
        }
    }
}