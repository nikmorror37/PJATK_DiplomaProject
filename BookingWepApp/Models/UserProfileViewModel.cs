using BookingWepApp.Areas.Identity.Pages.Account.Manage;
using System.Collections.Generic;

namespace BookingWepApp.Models
{
    public class UserProfileViewModel
    {
        //public IndexModel UserProfile { get; set; } // The existing IndexModel
        //public IEnumerable<Booking> Bookings { get; set; } // List of bookings
        
        public string Username { get; set; } //new
        public IndexModel.InputModel Input { get; set; } //new
        public List<Booking> Bookings { get; set; } = new List<Booking>(); //new
    }
}
