using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using BookingWepApp.Data;
using BookingWepApp.Models;
using System.Collections.Generic;

//namespace BookingWepApp.Areas.Identity.Pages.Account.Manage
//{
//    public partial class IndexModel : PageModel
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly IMongoCollection<ApplicationUser> _usersCollection;

//        public IndexModel(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager,
//            MongoDbContext mongoDbContext)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _usersCollection = mongoDbContext.Users;
//        }

//        [DisplayName("UserName")]
//        public string Username { get; set; }

//        [TempData]
//        public string StatusMessage { get; set; }

//        [BindProperty]
//        public InputModel Input { get; set; }

//        public class InputModel
//        {
//            [Phone]
//            [Display(Name = "Phone number")]
//            public string PhoneNumber { get; set; }

//            [Display(Name = "Name")]
//            public string FirstName { get; set; }

//            [Display(Name = "Surname")]
//            public string LastName { get; set; }

//            [Display(Name = "Address")]
//            public string Address { get; set; }

//            [Display(Name = "Index")]
//            public string PostalCode { get; set; }

//            [Display(Name = "City")]
//            public string City { get; set; }

//            [Display(Name = "Country")]
//            public string Country { get; set; }
//        }

//        private async Task LoadAsync(ApplicationUser user)
//        {
//            var userName = user.UserName;
//            var phoneNumber = user.PhoneNumber;

//            Username = userName;

//            Input = new InputModel
//            {
//                PhoneNumber = phoneNumber,
//                FirstName = user.FirstName,
//                LastName = user.LastName,
//                Address = user.Address,
//                PostalCode = user.PostalCode,
//                City = user.City,
//                Country = user.Country
//            };
//        }

//        public async Task<IActionResult> OnGetAsync()
//        {
//            var userId = _userManager.GetUserId(User);
//            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

//            if (user == null)
//            {
//                return NotFound($"User not found with ID '{userId}'.");
//            }

//            await LoadAsync(user);
//            return Page();
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            var userId = _userManager.GetUserId(User);
//            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

//            if (user == null)
//            {
//                return NotFound($"User not found with ID '{userId}'.");
//            }

//            if (!ModelState.IsValid)
//            {
//                await LoadAsync(user);
//                return Page();
//            }

//            if (Input.PhoneNumber != user.PhoneNumber)
//            {
//                user.PhoneNumber = Input.PhoneNumber;
//            }

//            if (Input.FirstName != user.FirstName)
//            {
//                user.FirstName = Input.FirstName;
//            }

//            if (Input.LastName != user.LastName)
//            {
//                user.LastName = Input.LastName;
//            }

//            if (Input.Address != user.Address)
//            {
//                user.Address = Input.Address;
//            }

//            if (Input.PostalCode != user.PostalCode)
//            {
//                user.PostalCode = Input.PostalCode;
//            }

//            if (Input.City != user.City)
//            {
//                user.City = Input.City;
//            }

//            if (Input.Country != user.Country)
//            {
//                user.Country = Input.Country;
//            }

//            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, user.Id);
//            await _usersCollection.ReplaceOneAsync(filter, user);

//            await _signInManager.RefreshSignInAsync(user);
//            StatusMessage = "User data changed";
//            return RedirectToPage();
//        }
//    }
//}

namespace BookingWepApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IMongoCollection<Booking> _bookingsCollection;
        private readonly IMongoCollection<Room> _roomsCollection;
        private readonly IMongoCollection<Hotel> _hotelsCollection;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            MongoDbContext mongoDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usersCollection = mongoDbContext.Users;
            _bookingsCollection = mongoDbContext.Bookings;
            _roomsCollection = mongoDbContext.Rooms;
            _hotelsCollection = mongoDbContext.Hotels;
        }

        [DisplayName("UserName")]
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<Booking> Bookings { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Name")]
            public string FirstName { get; set; }

            [Display(Name = "Surname")]
            public string LastName { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }

            [Display(Name = "Index")]
            public string PostalCode { get; set; }

            [Display(Name = "City")]
            public string City { get; set; }

            [Display(Name = "Country")]
            public string Country { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = user.UserName;
            var phoneNumber = user.PhoneNumber;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PostalCode = user.PostalCode,
                City = user.City,
                Country = user.Country
            };

            // Fetch user bookings
            var userId = user.Id;
            var userBookings = await _bookingsCollection.Find(b => b.UserId == userId).ToListAsync();

            // Supplement bookings with room and hotel details
            foreach (var booking in userBookings)
            {
                var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
                booking.Room = room;

                if (room != null)
                {
                    var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
                    booking.Room.Hotel = hotel;
                }
            }

            Bookings = userBookings;
        }

        //public async Task<IActionResult> OnGetAsync()
        //{
        //    var userId = _userManager.GetUserId(User);
        //    var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        //    if (user == null)
        //    {
        //        return NotFound($"User not found with ID '{userId}'.");
        //    }

        //    await LoadAsync(user);
        //    return Page();
        //}

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User not found with ID '{userId}'.");
            }

            Username = user.UserName;
            Input = new InputModel
            {
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PostalCode = user.PostalCode,
                City = user.City,
                Country = user.Country
            };

            return Page();
        }


        //public async Task<IActionResult> OnGetAsync()
        //{
        //    var userId = _userManager.GetUserId(User);
        //    var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        //    if (user == null)
        //    {
        //        return NotFound($"User not found with ID '{userId}'.");
        //    }

        //    await LoadAsync(user);

        //    // Populate the existing Bookings property
        //    Bookings = await _bookingsCollection.Find(b => b.UserId == userId).ToListAsync();

        //    // Fetch related Room and Hotel info
        //    foreach (var booking in Bookings)
        //    {
        //        var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
        //        booking.Room = room;

        //        if (room != null)
        //        {
        //            var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
        //            booking.Room.Hotel = hotel;
        //        }
        //    }

        //    return Page();
        //}

        //public async Task<IActionResult> OnGetAsync()
        //{
        //    // Fetch the user
        //    var userId = _userManager.GetUserId(User);
        //    var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        //    if (user == null)
        //    {
        //        return NotFound($"User not found with ID '{userId}'.");
        //    }

        //    // Fetch the bookings for the user
        //    var userBookings = await _bookingsCollection.Find(b => b.UserId == userId).ToListAsync();

        //    // Populate additional data for the bookings
        //    foreach (var booking in userBookings)
        //    {
        //        var room = await _roomsCollection.Find(r => r.Id == booking.RoomId).FirstOrDefaultAsync();
        //        booking.Room = room;

        //        if (room != null)
        //        {
        //            var hotel = await _hotelsCollection.Find(h => h.Id == room.HotelId).FirstOrDefaultAsync();
        //            booking.Room.Hotel = hotel;
        //        }
        //    }

        //    // Prepare the view model
        //    var viewModel = new UserProfileViewModel
        //    {
        //        Username = user.UserName,
        //        Input = new IndexModel.InputModel
        //        {
        //            PhoneNumber = user.PhoneNumber,
        //            FirstName = user.FirstName,
        //            LastName = user.LastName,
        //            Address = user.Address,
        //            PostalCode = user.PostalCode,
        //            City = user.City,
        //            Country = user.Country
        //        },
        //        Bookings = userBookings ?? new List<Booking>()
        //    };

        //    ViewData["UserProfile"] = viewModel;
        //    return Page();
        //}







        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User not found with ID '{userId}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = Input.PhoneNumber;
            }

            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }

            if (Input.Address != user.Address)
            {
                user.Address = Input.Address;
            }

            if (Input.PostalCode != user.PostalCode)
            {
                user.PostalCode = Input.PostalCode;
            }

            if (Input.City != user.City)
            {
                user.City = Input.City;
            }

            if (Input.Country != user.Country)
            {
                user.Country = Input.Country;
            }

            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, user.Id);
            await _usersCollection.ReplaceOneAsync(filter, user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "User data changed";
            return RedirectToPage();
        }
    }
}