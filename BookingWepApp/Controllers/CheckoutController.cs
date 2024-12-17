using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BookingWepApp.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingWepApp.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IMongoCollection<Booking> _bookingsCollection;
        private readonly IMongoCollection<Payment> _paymentsCollection;
        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IMongoCollection<Hotel> _hotelsCollection;
        private readonly IMongoCollection<Room> _roomsCollection;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(IMongoClient mongoClient, UserManager<ApplicationUser> userManager)
        {
            var database = mongoClient.GetDatabase("BookingDb");
            _bookingsCollection = database.GetCollection<Booking>("Bookings");
            _paymentsCollection = database.GetCollection<Payment>("Payments");
            _usersCollection = database.GetCollection<ApplicationUser>("Users");
            _hotelsCollection = database.GetCollection<Hotel>("Hotels");
            _roomsCollection = database.GetCollection<Room>("Rooms");
            _userManager = userManager;
        }

        public async Task<IActionResult> Checkout()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var identityClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                // Fetch session values
                var roomId = HttpContext.Session.GetString("roomId");
                var checkIn = HttpContext.Session.GetString("CheckInDate");
                var checkOut = HttpContext.Session.GetString("CheckOutDate");

                // Validate session values
                if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(checkIn) || string.IsNullOrEmpty(checkOut))
                {
                    throw new Exception("Session data is missing. RoomId, CheckInDate, or CheckOutDate is not set.");
                }

                // Fetch Room data
                var room = await _roomsCollection.Find(r => r.Id == roomId).FirstOrDefaultAsync();
                if (room == null)
                {
                    throw new Exception($"Room with ID '{roomId}' not found.");
                }

                // Fetch Hotel data
                var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
                if (hotel == null)
                {
                    throw new Exception($"Hotel associated with Room ID '{roomId}' not found.");
                }

                // Create Booking object
                var booking = new Booking
                {
                    RoomId = room.Id,
                    CheckIn = Convert.ToDateTime(checkIn),
                    CheckOut = Convert.ToDateTime(checkOut),
                    Status = Status.Pending,
                    UserId = identityClaim.Value
                };

                // Insert Booking into the database
                await _bookingsCollection.InsertOneAsync(booking);

                // Calculate Total Price
                var numberOfNights = (booking.CheckOut - booking.CheckIn).TotalDays;
                var totalPrice = decimal.Round(room.RoomPrice * (decimal)numberOfNights, 2);

                ViewBag.RoomPrice = room.RoomPrice;
                ViewBag.NumberOfNights = numberOfNights;
                ViewBag.TotalPrice = totalPrice;

                HttpContext.Session.SetString("bookingId", booking.Id);

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Log errors
                return BadRequest("An error occurred while processing the booking.");
            }
        }




        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Checkout(Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.Status = Status.Pending;
                payment.Date = DateTime.Now;
                payment.CardNumber = payment.CardNumber.Substring(12);
                payment.CVV = payment.CVV.Substring(0, 1);

                await _paymentsCollection.InsertOneAsync(payment);

                return RedirectToAction("PaymentCheckout", new { paymentId = payment.Id });
            }

            return View();
        }

        public async Task<IActionResult> PaymentCheckout(string paymentId)
        {
            var payment = await _paymentsCollection.Find(p => p.Id == paymentId).FirstOrDefaultAsync();

            if (payment != null)
            {
                var bookingId = HttpContext.Session.GetString("bookingId");
                var booking = await _bookingsCollection.Find(b => b.Id == bookingId).FirstOrDefaultAsync();

                booking.Status = Status.Accepted;
                booking.PaymentId = payment.Id;

                var updateBooking = Builders<Booking>.Update
                    .Set(b => b.Status, booking.Status)
                    .Set(b => b.PaymentId, booking.PaymentId);

                await _bookingsCollection.UpdateOneAsync(b => b.Id == bookingId, updateBooking);

                var updatePayment = Builders<Payment>.Update.Set(p => p.Status, Status.Accepted);
                await _paymentsCollection.UpdateOneAsync(p => p.Id == payment.Id, updatePayment);

                return RedirectToAction("BookingConfirmation", new { paymentId = payment.Id });
            }

            return View();
        }

        public async Task<IActionResult> BookingConfirmation(string paymentId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var identityClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var user = await _usersCollection.Find(u => u.Id == identityClaim.Value).FirstOrDefaultAsync();
            var bookingId = HttpContext.Session.GetString("bookingId");
            var booking = await _bookingsCollection.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
            var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();

            var bookingConfVM = new BookingConfirmationViewModel
            {
                User = user,
                Payment = await _paymentsCollection.Find(p => p.Id == paymentId).FirstOrDefaultAsync(),
                Booking = booking,
                Room = room,
                Hotel = hotel
            };

            return View(bookingConfVM);
        }
    }
}
