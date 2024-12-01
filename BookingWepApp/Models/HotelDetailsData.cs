using System;

namespace BookingWepApp.Models
{
    public static class HotelDetailsData
    {
        public static Hotel CurrentHotel { get; set; }
        public static DateTime CheckInDate { get; set; }
        public static DateTime CheckOutDate { get; set; }
    }
}