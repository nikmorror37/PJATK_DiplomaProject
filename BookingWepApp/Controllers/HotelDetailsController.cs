using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BookingWepApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BookingWepApp.Controllers
{
    [Authorize]
    public class HotelDetailsController : Controller
    {
        private readonly IMongoCollection<Hotel> _hotelsCollection;
        private readonly IMongoCollection<Room> _roomsCollection;
        private readonly IMongoCollection<Booking> _bookingsCollection;
        private readonly UserManager<ApplicationUser> _userManager;

        public HotelDetailsController(IMongoClient mongoClient, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            // Retrieve the database name from the configuration
            var databaseName = configuration.GetSection("MongoDbSettings:DatabaseName").Value;
            var database = mongoClient.GetDatabase(databaseName);

            // Initialize collections
            _hotelsCollection = database.GetCollection<Hotel>("Hotels");
            _roomsCollection = database.GetCollection<Room>("Rooms");
            _bookingsCollection = database.GetCollection<Booking>("Bookings");

            _userManager = userManager;
        }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        public ApplicationUser CurrentUser { get; set; }

        public async Task<IActionResult> HotelPage(string id)
        {
            var hotel = await _hotelsCollection.Find(h => h.Id == id).FirstOrDefaultAsync();
            if (hotel == null)
                return NotFound();

            var rooms = await _roomsCollection.Find(r => r.HotelId == id).ToListAsync();
            hotel.Rooms = rooms;

            HotelDetailsData.CurrentHotel = hotel;

            return View(hotel);
        }

        public async Task<IActionResult> BookRoom(RoomType roomType, string id)
        {
            try
            {
                // Fetch current user
                CurrentUser = await _userManager.GetUserAsync(User);
                if (CurrentUser == null)
                {
                    return Unauthorized("User not logged in.");
                }

                // Find an available room
                var availableRoomToBeBooked = await _roomsCollection.Find(r => r.HotelId == id && r.RoomType == roomType).FirstOrDefaultAsync();
                if (availableRoomToBeBooked == null)
                {
                    return NotFound("No available room found for the selected hotel and room type.");
                }

                // Save booking data in session
                HttpContext.Session.SetString("roomId", availableRoomToBeBooked.Id);
                HttpContext.Session.SetString("CheckInDate", HotelDetailsData.CheckInDate.ToString("yyyy-MM-dd"));
                HttpContext.Session.SetString("CheckOutDate", HotelDetailsData.CheckOutDate.ToString("yyyy-MM-dd"));

                return RedirectToAction("Checkout", "Checkout");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Log errors
                return BadRequest("An error occurred while booking the room.");
            }
        }



        private List<DateTime> GetStayingDaysRangeList()
        {
            var daysRangeList = new List<DateTime>();
            for (DateTime date = HotelDetailsData.CheckInDate; date <= HotelDetailsData.CheckOutDate; date = date.AddDays(1))
            {
                daysRangeList.Add(date);
            }
            return daysRangeList;
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SearchRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkInDate.Year <= 1 || checkOutDate.Year <= 1)
            {
                ViewBag.Error = "Specify check-in and check-out dates";
                return View("HotelPage", HotelDetailsData.CurrentHotel);
            }

            if (checkOutDate <= checkInDate)
            {
                ViewBag.Error = "The check-out date must be strictly greater than the check-in date";
                return View("HotelPage", HotelDetailsData.CurrentHotel);
            }

            var hotelId = HotelDetailsData.CurrentHotel.Id;

            // Fetch all rooms for the hotel
            var allRooms = await _roomsCollection.Find(r => r.HotelId == hotelId).ToListAsync();

            if (allRooms.Count == 0)
            {
                ViewBag.Error = "No rooms available in this hotel.";
                return View("HotelPage", HotelDetailsData.CurrentHotel);
            }

            // Get the IDs of all rooms in this hotel
            var roomIds = allRooms.Select(r => r.Id).ToList();

            // Find all bookings for rooms in this hotel during the specified date range
            var bookedRooms = await _bookingsCollection.Find(b =>
                roomIds.Contains(b.RoomId) && // Match rooms in this hotel
                b.CheckIn < checkOutDate &&   // Booking starts before the check-out date
                b.CheckOut > checkInDate      // Booking ends after the check-in date
            ).ToListAsync();

            if (bookedRooms.Count == 0)
            {
                ViewBag.Error = "All rooms are available.";
            }

            // Group rooms by RoomType and calculate availability
            var roomItems = allRooms
                .GroupBy(r => r.RoomType)
                .Select(gr =>
                {
                    var bookedCount = bookedRooms.Count(br => gr.Any(r => r.Id == br.RoomId));
                    return new RoomItem
                    {
                        RoomType = gr.Key,
                        TotalRooms = gr.Count(),
                        AvailableRooms = gr.Count() - bookedCount
                    };
                })
                .ToList();

            // Pass data to the view
            ViewBag.RoomsAvailable = roomItems;
            ViewBag.DataInfo = new List<DateTime>() { checkInDate, checkOutDate };

            // Save the dates for later use
            HotelDetailsData.CheckInDate = checkInDate;
            HotelDetailsData.CheckOutDate = checkOutDate;

            return View("HotelPage", HotelDetailsData.CurrentHotel);
        }

        public class RoomItem
        {
            public RoomType RoomType { get; set; }
            public int AvailableRooms { get; set; }
            public int TotalRooms { get; set; }
        }
    }
}
