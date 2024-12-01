using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Booking
    /// </summary>
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("RoomId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoomId { get; set; }

        [BsonIgnore]
        public Room Room { get; set; }

        [BsonElement("CheckIn")]
        public DateTime CheckIn { get; set; }

        [BsonElement("CheckOut")]
        public DateTime CheckOut { get; set; }

        [BsonElement("Status")]
        public Status Status { get; set; }

        [BsonElement("UserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonIgnore]
        public ApplicationUser User { get; set; }

        [BsonElement("PaymentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PaymentId { get; set; }

        [BsonIgnore]
        public Payment Payment { get; set; }
    }

    public enum Status
    {
        Accepted,
        Denied,
        Pending,
        Error
    }
}
