using BookingWepApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingWepApp.Controllers
{
    [Authorize]
    public class MyBookingsController : Controller
    {
        private readonly IMongoCollection<Booking> _bookingsCollection;
        private readonly IMongoCollection<Room> _roomsCollection;
        private readonly IMongoCollection<Hotel> _hotelsCollection;

        public MyBookingsController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("BookingDb");
            _bookingsCollection = database.GetCollection<Booking>("Bookings");
            _roomsCollection = database.GetCollection<Room>("Rooms");
            _hotelsCollection = database.GetCollection<Hotel>("Hotels");
        }

        // Метод для отображения всех бронирований текущего пользователя
        public async Task<IActionResult> Index()
        {
            // Получаем текущего пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Находим все бронирования пользователя
            var userBookings = await _bookingsCollection.Find(b => b.UserId == userId).ToListAsync();

            // Дополняем данные о бронированиях информацией о номерах и отелях
            foreach (var booking in userBookings)
            {
                // Ищем номер, связанный с бронированием
                var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
                booking.Room = room;

                // Ищем отель, связанный с номером
                if (room != null)
                {
                    var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
                    booking.Room.Hotel = hotel;
                }
            }

            // Возвращаем представление с данными о бронированиях
            return View(userBookings);
        }
    }
}
