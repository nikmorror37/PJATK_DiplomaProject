using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Security.Claims;

namespace BookingWepApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string Id { get; set; }

        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [BsonElement("LastName")]
        public string LastName { get; set; }

        //[BsonElement("Email")]
        //public override string Email { get; set; } //think about override

        //[BsonElement("UserName")]
        //public override string UserName { get; set; } //think about it

        //[BsonElement("PhoneNumber")]
        //public override string PhoneNumber { get; set; } //think

        [BsonElement("Address")]
        public string Address { get; set; }

        [BsonElement("PostalCode")]
        public string PostalCode { get; set; }

        [BsonElement("City")]
        public string City { get; set; }

        [BsonElement("Country")]
        public string Country { get; set; }

        //[BsonElement("IsMember")]
        //public bool IsMember { get; set; }

        //[BsonElement("PasswordHash")]
        //public override string PasswordHash { get; set; } //think

        [BsonIgnoreIfNull]
        public ICollection<Booking> Bookings { get; set; }

        //// Add the required properties for Identity
        //[BsonElement("NormalizedUserName")]
        //public string NormalizedUserName { get; set; }

        [BsonElement("Roles")]
        public List<string> Roles { get; set; } = new List<string>();
        public List<Claim> Claims { get; internal set; }
    }
}
