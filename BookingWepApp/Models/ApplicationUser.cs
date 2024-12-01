using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BookingWepApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public bool IsMember { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}