using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Room
    /// </summary>
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Room type")]
        public RoomType RoomType { get; set; }

        [Required]
        [Range(0, double.PositiveInfinity)]
        [Display(Name = "Cost per night")]
        public decimal RoomPrice { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Description (max. 255 characters)")]
        public string RoomDescription { get; set; }

        [Required]
        [Display(Name = "Number of beds")]
        [Range(1, 5)]
        public int NumberOfBeds { get; set; }

        [Required]
        [Display(Name = "Capacity")]
        [Range(1, 7)]
        public int Capacity { get; set; }

        [Display(Name = "Photo of the room")]
        public string RoomImageUrl { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        [Display(Name = "Hotel")]
        public string HotelId { get; set; }

        [BsonIgnore]
        public Hotel Hotel { get; set; }

        [BsonIgnore]
        public ICollection<Booking> Bookings { get; set; }
    }

    public enum RoomType
    {
        Single,
        Double,
        Twin,
        Triple
    }
}
