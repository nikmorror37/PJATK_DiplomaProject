using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Hotel
    /// </summary>
    public class Hotel
    {
        [BsonId] // Указывает, что это поле является идентификатором MongoDB
        [BsonRepresentation(BsonType.ObjectId)] // Автоматически преобразует строку в ObjectId
        public string Id { get; set; }

        [Required]
        [Display(Name = "Hotel name")]
        [StringLength(30)]
        public string Name { get; set; }

        [Required]
        [Url]
        [DisplayName("Website URL")]
        public string Website { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9- ]*$", ErrorMessage = "Only letters, numbers, hyphen (-), and spaces are allowed.")]
        [Display(Name = "Postal Code")]
        public string ZipCode { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Description (max. 255 characters)")]
        public string Description { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Stars")]
        public int Stars { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Distance from City Center")]
        public double DistanceFromCenter { get; set; }

        [DisplayName("Hotel image")]
        public string ImageUrl { get; set; }

        // Список связанных комнат
        public ICollection<Room> Rooms { get; set; }
    }
}
