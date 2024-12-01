using BookingWepApp.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingWepApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsApiController : ControllerBase
    {
        private readonly IMongoCollection<Hotel> _hotelsCollection;

        public HotelsApiController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("BookingDb");
            _hotelsCollection = database.GetCollection<Hotel>("Hotels");
        }

        // Метод GET: api/HotelsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hotel>>> Get()
        {
            // Возвращает список всех отелей
            var hotels = await _hotelsCollection.Find(_ => true).ToListAsync();
            return Ok(hotels);
        }

        // Метод GET: api/HotelsApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Hotel>> Get(string id)
        {
            // Ищем отель по ID
            var hotel = await _hotelsCollection.Find(h => h.Id == id).FirstOrDefaultAsync();
            if (hotel == null)
            {
                // Ошибка 404, если отель не найден
                return NotFound();
            }
            return Ok(hotel);
        }

        // Метод POST: api/HotelsApi
        [HttpPost]
        public async Task<ActionResult<Hotel>> Post(Hotel item)
        {
            // Добавляем отель в коллекцию
            await _hotelsCollection.InsertOneAsync(item);
            // Возвращаем статус 201 Created
            return CreatedAtAction(
                nameof(Get),
                new { id = item.Id },
                item);
        }

        // Метод PUT: api/HotelsApi/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, Hotel item)
        {
            // Проверяем совпадение ID
            if (id != item.Id)
            {
                return BadRequest("ID in the URL does not match ID in the body.");
            }

            // Ищем отель по ID
            var result = await _hotelsCollection.ReplaceOneAsync(h => h.Id == id, item);
            if (result.MatchedCount == 0)
            {
                // Ошибка 404, если отель не найден
                return NotFound();
            }

            // Успешное обновление
            return NoContent();
        }

        // Метод DELETE: api/HotelsApi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // Удаляем отель из коллекции
            var result = await _hotelsCollection.DeleteOneAsync(h => h.Id == id);
            if (result.DeletedCount == 0)
            {
                // Ошибка 404, если отель не найден
                return NotFound();
            }

            // Успешное удаление
            return NoContent();
        }

        // Проверка существования отеля
        private async Task<bool> ItemExists(string id)
        {
            var count = await _hotelsCollection.CountDocumentsAsync(h => h.Id == id);
            return count > 0;
        }
    }
}
