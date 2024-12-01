using System;
using System.ComponentModel.DataAnnotations;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Бронирование
    /// </summary>
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public Status Status { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int? PaymentId { get; set; }
        public Payment Payment { get; set; }
    }

    public enum Status { Accepted, Denied, Pending, Error }
}