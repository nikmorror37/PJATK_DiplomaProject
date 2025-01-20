using MongoDB.Driver;
using BookingWepApp.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BookingWepApp.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            // Retrieve connection string and database name from configuration
            var connectionString = configuration.GetValue<string>("MongoDbSettings:ConnectionString");
            var databaseName = configuration.GetValue<string>("MongoDbSettings:DatabaseName");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string for MongoDB is not configured.");
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName), "Database name for MongoDB is not configured.");
            }

            // Инициализация клиента MongoDB с использованием строки подключения из appsettings.json
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }
        public IMongoCollection<IdentityRole> Roles => _database.GetCollection<IdentityRole>("Roles");

        // Коллекции MongoDB для каждой сущности
        // Collections
        public IMongoCollection<Hotel> Hotels => _database.GetCollection<Hotel>("Hotels");
        public IMongoCollection<Room> Rooms => _database.GetCollection<Room>("Rooms");
        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
        public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("Payments");
        public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("Users");


        public void SeedData()
        {
            // Clear existing data from collections
            Rooms.DeleteMany(Builders<Room>.Filter.Empty); // Clears the Rooms collection
            Hotels.DeleteMany(Builders<Hotel>.Filter.Empty); // Clears the Hotels collection, if needed

            // Проверяем, если коллекция Rooms пуста
            if (Rooms.EstimatedDocumentCount() == 0)
            {
                // Проверяем, если коллекция Hotels пуста
                if (Hotels.EstimatedDocumentCount() == 0)
                {
                    // Add test hotels
                    var hotels = new List<Hotel>
                    {
                        new Hotel
                        {
                            Name = "Ritz Hotel Paris",
                            Website = "https://www.ritzparis.com/en-GB",
                            Address = "15 Pl. Vendome",
                            ZipCode = "75001",
                            City = "Paris",
                            Country = "France",
                            Description = "Hotel Ritz is located in Paris, 500 meters from the Opera Garnier. It offers several bars and restaurants, a garden and a business center.",
                            Stars = 5,
                            DistanceFromCenter = 8.3,
                            ImageUrl = "~/pictures/hotels/hotelRitzParis.jpg"
                        },
                        new Hotel
                        {
                            Name = "Corinthia Hotel London",
                            Website = "https://www.corinthia.com/london/",
                            Address = "Whitehall Pl.",
                            ZipCode = "SW1A 2BD",
                            City = "London",
                            Country = "Great Britain",
                            Description = "The luxurious Corinthia Hotel is located in one of London's most prestigious neighborhoods, steps from Trafalgar Square and Whitehall.",
                            Stars = 5,
                            DistanceFromCenter = 1.6,
                            ImageUrl = "~/pictures/hotels/hotel_corinthia.jpg"
                        },
                        new Hotel
                        {
                            Name = "Leonardo Hotel Amsterdam",
                            Website = "https://www.leonardo-hotels.com/amsterdam/leonardo-hotel-amsterdam-city-center/",
                            Address = "Tesselschadestraat 23",
                            ZipCode = "1054 ET",
                            City = "Amsterdam",
                            Country = "Netherlands",
                            Description = "Experience the essence of Amsterdam at the Leonardo Hotel Amsterdam City Center.",
                            Stars = 4,
                            DistanceFromCenter = 1.6,
                            ImageUrl = "~/pictures/hotels/hotel_leonardo.jpg"
                        },
                        new Hotel
                        {
                            Name = "Bristol Hotel Warsaw",
                            Website = "https://www.hotelbristolwarsaw.pl/",
                            Address = "Krakowskie Przedmiescie 42/44",
                            ZipCode = "00-325",
                            City = "Warsaw",
                            Country = "Poland",
                            Description = "A historic five-star luxury hotel built in the Neo-Renaissance style and opened in 1901 in Warsaw, Poland. It is located in the city centre next to the Presidential Palace.",
                            Stars = 5,
                            DistanceFromCenter = 0.5,
                            ImageUrl = "~/pictures/hotels/hotel_bristol.jpg"
                        },

                    };

                    Hotels.InsertMany(hotels);

                    // Add test rooms
                    var random = new Random();
                    var roomDescriptionArray = new[]
                    {
                        "Traditionally decorated room with TV, tea/coffee making facilities and en-suite shower room.",
                        "Double room with 1 bed, air conditioning, flat-screen TV, direct-dial telephone, high-speed Wi-Fi, hairdryer and safe. The modern bathroom has a power shower.",
                        "This room has satellite TV, tea/coffee making facilities, a hairdryer and a private bathroom.",
                        "Triple room with air conditioning, electric kettle, free Wi-Fi and hairdryer."
                    };
                    var numberOfBedsArray = new[] { 1, 1, 2, 3 };
                    var capacityArray = new[] { 1, 2, 2, 3 };
                    var roomImagesArray = new[]
                    {
                        "~/pictures/rooms/single.jpg",
                        "~/pictures/rooms/double.jpg",
                        "~/pictures/rooms/twin.jpg",
                        "~/pictures/rooms/triple.jpg",
                    };

                    var rooms = new List<Room>();
                    for (int i = 0; i < 100; i++)
                    {
                        var roomTypeIndex = random.Next(0, 4); // Тип комнаты (0 - Single, 1 - Double, 2 - Twin, 3 - Triple)
                        var randomHotel = hotels[random.Next(0, hotels.Count)]; // Случайный отель из списка

                        rooms.Add(new Room
                        {
                            RoomType = (RoomType)roomTypeIndex,
                            RoomPrice = (roomTypeIndex + 1) * 100, // Цена зависит от типа номера
                            RoomDescription = roomDescriptionArray[roomTypeIndex],
                            NumberOfBeds = numberOfBedsArray[roomTypeIndex],
                            Capacity = capacityArray[roomTypeIndex],
                            RoomImageUrl = roomImagesArray[roomTypeIndex],
                            HotelId = randomHotel.Id // Привязываем к ID отеля
                        });
                    }

                    Rooms.InsertMany(rooms);
                }
            }
        }
    }
}
