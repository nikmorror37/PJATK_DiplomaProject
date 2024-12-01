using MongoDB.Driver;
using BookingWepApp.Models;
using System;
using System.Collections.Generic;

namespace BookingWepApp.Data
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;

        public ApplicationDbContext()
        {
            // Подключение к MongoDB
            var connectionString = "mongodb://localhost:27017"; // Локальный адрес MongoDB сервера
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("BookingOne"); // Имя базы данных
        }

        // Коллекции MongoDB для каждой сущности
        public IMongoCollection<Hotel> Hotels => _database.GetCollection<Hotel>("Hotels");
        public IMongoCollection<Room> Rooms => _database.GetCollection<Room>("Rooms");
        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
        public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("Payments");
        public IMongoCollection<ApplicationUser> ApplicationUsers => _database.GetCollection<ApplicationUser>("ApplicationUsers");

        // Метод для заполнения базы данных тестовыми данными
        public void SeedData()
        {
            // Проверяем, если данные уже существуют
            if (Hotels.CountDocuments(_ => true) == 0)
            {
                // Добавляем тестовые отели
                var hotels = new List<Hotel>
                {
                    new Hotel
                    {
                        Id = 1,
                        Name = "Ритц Отель Париж",
                        Website = "https://www.ritzparis.com/en-GB",
                        Address = "15 Вандомская площадь",
                        ZipCode = "75001",
                        City = "Париж",
                        Country = "Франция",
                        Description = "Отель Ritz Paris расположен в Париже, в 500 метрах от Оперы Гарнье. К услугам гостей несколько баров и ресторанов, сад и бизнес-центр.",
                        Stars = 5,
                        DistanceFromCenter = 8.3,
                        ImageUrl = "~/pictures/hotels/hotel1.jpg"
                    },
                    new Hotel
                    {
                        Id = 2,
                        Name = "Коринтия Отель Лондон",
                        Website = "https://www.corinthia.com/london/",
                        Address = "Уайтхолл Плейс",
                        ZipCode = "SW1A 2BD",
                        City = "Лондон",
                        Country = "Великобритания",
                        Description = "Роскошный отель Corinthia расположен в одном из самых престижных районов Лондона, в нескольких шагах от Трафальгарской площади и Уайтхолла.",
                        Stars = 5,
                        DistanceFromCenter = 1.6,
                        ImageUrl = "~/pictures/hotels/hotel2.jpg"
                    },
                    new Hotel
                    {
                        Id = 3,
                        Name = "Леонардо Отель Амстердам",
                        Website = "https://www.leonardo-hotels.com/amsterdam/leonardo-hotel-amsterdam-city-center/",
                        Address = "Тессельшадестраат 23",
                        ZipCode = "1054 ET",
                        City = "Амстердам",
                        Country = "Нидерланды",
                        Description = "Почувствуйте сущность Амстердама в отеле Leonardo Hotel Amsterdam City Center.",
                        Stars = 4,
                        DistanceFromCenter = 1.6,
                        ImageUrl = "~/pictures/hotels/hotel3.jpg"
                    }
                };

                Hotels.InsertMany(hotels);

                // Добавляем тестовые комнаты
                var random = new Random();
                var roomDescriptionArray = new[]
                {
                    "Традиционно оформленный номер с телевизором, принадлежностями для чая/кофе и собственной душевой.",
                    "Двухместный номер с 1 кроватью, кондиционером, телевизором с плоским экраном, телефоном с прямым набором номера, высокоскоростным Wi-Fi, феном и сейфом. В современной ванной комнате установлен душ с сильным напором воды.",
                    "В этом номере есть спутниковое телевидение, принадлежности для чая/кофе, фен и собственная ванная комната.",
                    "Трехместный номер с кондиционером, электрическим чайником, бесплатным Wi-Fi и феном."
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
                    var randomHotelId = random.Next(1, 4); // Случайный отель

                    rooms.Add(new Room
                    {
                        Id = i + 1,
                        RoomType = (RoomType)roomTypeIndex,
                        RoomPrice = (roomTypeIndex + 1) * 100 + randomHotelId * 50, // Цена зависит от типа номера и отеля
                        RoomDescription = roomDescriptionArray[roomTypeIndex],
                        NumberOfBeds = numberOfBedsArray[roomTypeIndex],
                        Capacity = capacityArray[roomTypeIndex],
                        RoomImageUrl = roomImagesArray[roomTypeIndex], // Правильное изображение для типа комнаты
                        HotelId = randomHotelId
                    });
                }

                Rooms.InsertMany(rooms);
            }
        }
    }
}
