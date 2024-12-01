using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BookingWepApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingWepApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminRoomsController : Controller
    {
        private readonly IMongoCollection<Room> _roomsCollection;
        private readonly IMongoCollection<Hotel> _hotelsCollection;

        public AdminRoomsController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("BookingDb");
            _roomsCollection = database.GetCollection<Room>("Rooms");
            _hotelsCollection = database.GetCollection<Hotel>("Hotels");
        }

        public async Task<IActionResult> Index(string id)
        {
            var roomsQuery = _roomsCollection.AsQueryable();

            if (!string.IsNullOrEmpty(id))
            {
                roomsQuery = roomsQuery.Where(r => r.Id == id);
            }

            var rooms = await _roomsCollection.Find(_ => true).ToListAsync();
            return View(rooms);
        }

        public async Task<IActionResult> Create()
        {
            var hotels = await _hotelsCollection.Find(_ => true).ToListAsync();
            ViewData["HotelId"] = new SelectList(hotels, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomType,RoomPrice,RoomDescription,NumberOfBeds,Capacity,RoomImageUrl,HotelId")] Room room)
        {
            if (ModelState.IsValid)
            {
                await _roomsCollection.InsertOneAsync(room);
                return RedirectToAction(nameof(Index));
            }

            var hotels = await _hotelsCollection.Find(_ => true).ToListAsync();
            ViewData["HotelId"] = new SelectList(hotels, "Id", "Name", room.HotelId);
            return View(room);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var room = await _roomsCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (room == null)
            {
                return NotFound();
            }

            var hotels = await _hotelsCollection.Find(_ => true).ToListAsync();
            ViewData["HotelId"] = new SelectList(hotels, "Id", "Name", room.HotelId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,RoomType,RoomPrice,RoomDescription,NumberOfBeds,Capacity,RoomImageUrl,HotelId")] Room room)
        {
            if (ModelState.IsValid)
            {
                var update = Builders<Room>.Update
                    .Set(r => r.RoomType, room.RoomType)
                    .Set(r => r.RoomPrice, room.RoomPrice)
                    .Set(r => r.RoomDescription, room.RoomDescription)
                    .Set(r => r.NumberOfBeds, room.NumberOfBeds)
                    .Set(r => r.Capacity, room.Capacity)
                    .Set(r => r.RoomImageUrl, room.RoomImageUrl)
                    .Set(r => r.HotelId, room.HotelId);

                await _roomsCollection.UpdateOneAsync(r => r.Id == id, update);
                return RedirectToAction(nameof(Index));
            }

            var hotels = await _hotelsCollection.Find(_ => true).ToListAsync();
            ViewData["HotelId"] = new SelectList(hotels, "Id", "Name", room.HotelId);
            return View(room);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var room = await _roomsCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _roomsCollection.DeleteOneAsync(r => r.Id == id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RoomExists(string id)
        {
            var count = await _roomsCollection.CountDocumentsAsync(r => r.Id == id);
            return count > 0;
        }
    }
}
