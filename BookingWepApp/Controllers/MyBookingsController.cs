using BookingWepApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
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
        private readonly IMongoCollection<Payment> _paymentsCollection;

        public MyBookingsController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("BookingDb");
            _bookingsCollection = database.GetCollection<Booking>("Bookings");
            _roomsCollection = database.GetCollection<Room>("Rooms");
            _hotelsCollection = database.GetCollection<Hotel>("Hotels");
            _paymentsCollection = database.GetCollection<Payment>("Payments");
        }

        //// Метод для отображения всех бронирований текущего пользователя
        //public async Task<IActionResult> Index()
        //{
        //    // Получаем текущего пользователя
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Находим все бронирования пользователя
        //    var userBookings = await _bookingsCollection.Find(b => b.UserId == userId).ToListAsync();

        //    // Дополняем данные о бронированиях информацией о номерах и отелях
        //    foreach (var booking in userBookings)
        //    {
        //        // Ищем номер, связанный с бронированием
        //        var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
        //        booking.Room = room;

        //        // Ищем отель, связанный с номером
        //        if (room != null)
        //        {
        //            var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
        //            booking.Room.Hotel = hotel;
        //        }
        //    }

        //    // Возвращаем представление с данными о бронированиях
        //    return View(userBookings);
        //}

        //Second variant
        //public async Task<IActionResult> Index()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    //var userId = User.FindFirst("sub")?.Value;

        //    // Fetch user's bookings
        //    var userBookings = await _bookingsCollection.Find(b => b.UserId == userId).ToListAsync();

        //    //// Populate additional data
        //    //foreach (var booking in userBookings)
        //    //{
        //    //    var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
        //    //    booking.Room = room; // Ensure room is not null

        //    //    if (room != null)
        //    //    {
        //    //        var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
        //    //        booking.Room.Hotel = hotel; // Ensure hotel is not null
        //    //    }
        //    //    else
        //    //    {
        //    //        booking.Room = new Room
        //    //        {
        //    //            RoomDescription = "Room information not available"
        //    //        };
        //    //    }

        //    //    var payment = await _paymentsCollection.Find(p => p.Id == booking.PaymentId).FirstOrDefaultAsync();
        //    //    booking.Payment = payment;
        //    //}

        //    foreach (var booking in userBookings)
        //    {
        //        // Fetch room details
        //        var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
        //        booking.Room = room;

        //        // Fetch hotel details
        //        if (room != null)
        //        {
        //            var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
        //            if (hotel != null)
        //            {
        //                booking.Room.Hotel = hotel;
        //            }
        //        }

        //        // Fetch payment details
        //        var payment = await _paymentsCollection.Find(p => p.Id == booking.PaymentId).FirstOrDefaultAsync();
        //        if (payment != null)
        //        {
        //            booking.Payment = payment;
        //        }
        //    }


        //    return View(userBookings);
        //}

        public async Task<IActionResult> Index()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var identityClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                // Fetch all bookings for the current user
                var userBookings = await _bookingsCollection.Find(b => b.UserId == identityClaim.Value).ToListAsync();

                // Populate additional data for each booking
                foreach (var booking in userBookings)
                {
                    var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
                    booking.Room = room;

                    if (room != null)
                    {
                        var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
                        booking.Room.Hotel = hotel;
                    }
                    var payment = await _paymentsCollection.Find(p => p.Id == booking.PaymentId).FirstOrDefaultAsync();
                    if (payment != null)
                    {
                        booking.Payment = payment;
                    }
                }

                return View(userBookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Log errors
                return BadRequest("An error occurred while fetching bookings.");
            }
        }


    }
}
