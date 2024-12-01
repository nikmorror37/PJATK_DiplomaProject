using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Payment
    /// </summary>
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public Status Status { get; set; }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Type of credit card")]
        public CardType Type { get; set; }

        [Required]
        [Display(Name = "Credit card number")]
        [RegularExpression(@"^([0-9]{16})$", ErrorMessage = "Enter the 16-digit credit card number")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"^([0-9]{3})$", ErrorMessage = "CVV code is not correct")]
        public string CVV { get; set; }

        [Required]
        [Display(Name = "Name on the credit card")]
        public string CardHolderFirstName { get; set; }

        [Required]
        [Display(Name = "Surname on the credit card")]
        public string CardHolderLastName { get; set; }

        [Required]
        [Display(Name = "Expiration date")]
        public string ExpirationDate { get; set; }

        [BsonIgnore]
        public Booking Booking { get; set; }
    }

    public enum CardType
    {
        Visa,
        MasterCard
    }
}
