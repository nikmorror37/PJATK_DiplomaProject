﻿namespace BookingWepApp.Models.Infrastructure
{
    public class RoomTypeDecoder
    {
        public static string GetRoomTypeName(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Single:
                    return "Одноместный";
                case RoomType.Double:
                    return "Двухместный";
                case RoomType.Triple:
                    return "Трехместный";
                case RoomType.Twin:
                    return "Пара (одна кровать на двоих)";
                default:
                    return "Другой";
            }
        }
    }
}