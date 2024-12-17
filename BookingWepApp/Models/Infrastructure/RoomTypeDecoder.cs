namespace BookingWepApp.Models.Infrastructure
{
    public class RoomTypeDecoder
    {
        public static string GetRoomTypeName(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Single:
                    return "Jednoosobowy";
                case RoomType.Double:
                    return "Para (jedno łóżko dla dwóch osób)";
                case RoomType.Triple:
                    return "Trzyosobowy";
                case RoomType.Twin:
                    return "Dwuosobowy";
                default:
                    return "Drugi";
            }
        }
    }
}