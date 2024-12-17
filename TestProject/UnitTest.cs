using BookingWepApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using System;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class UnitTest
    {
        private IMongoDatabase _database;
        private IMongoCollection<Hotel> _hotelsCollection;
        private IMongoCollection<Room> _roomsCollection;
        private IMongoCollection<ApplicationUser> _usersCollection;

        [TestInitialize]
        public void Initialize()
        {
            var client = new MongoClient("mongodb://localhost:27017"); // current project connection 
            _database = client.GetDatabase("BookingDb");

            _hotelsCollection = _database.GetCollection<Hotel>("Hotels");
            _roomsCollection = _database.GetCollection<Room>("Rooms");
            _usersCollection = _database.GetCollection<ApplicationUser>("Users");
        }

        [TestMethod]
        public void TryToGetHotels()
        {
            var list = _hotelsCollection.Find(_ => true).ToList();
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);

            Console.WriteLine("All hotels:");
            foreach (var hotel in list)
            {
                Console.WriteLine($"Name: {hotel.Name}, City: {hotel.City}, Country: {hotel.Country}, Stars: {hotel.Stars}");
            }
        }

        [TestMethod]
        public void TryToCreateHotel()
        {
            var hotel = new Hotel()
            {
                Name = "Istanbul Hotel",
                Address = "Some Street, 12",
                City = "Istanbul",
                Country = "Turkey",
                Description = "A test hotel for MongoDB",
                DistanceFromCenter = 10.0,
                Stars = 4,
                ZipCode = "238493",
                Website = "https://galataistanbulhotel.com/"
            };

            _hotelsCollection.InsertOne(hotel);

            var insertedHotel = _hotelsCollection
                .Find(h => h.Name == "Istanbul Hotel")
                .FirstOrDefault();

            Assert.IsNotNull(insertedHotel);
            Assert.AreEqual("Istanbul Hotel", insertedHotel.Name);
        }

        [TestMethod]
        public void TryToGetRoom()
        {
            var room = _roomsCollection.Find(_ => true).FirstOrDefault();
            Assert.IsNotNull(room);

            Console.WriteLine("Room info:");
            Console.WriteLine($"Room Type: {room.RoomType}");
            Console.WriteLine($"Price per Night: {room.RoomPrice} PLN");
            Console.WriteLine($"Number of Beds: {room.NumberOfBeds}");
            Console.WriteLine($"Capacity: {room.Capacity}");
            Console.WriteLine($"Description: {room.RoomDescription}");
            Console.WriteLine($"Hotel ID: {room.HotelId}");
        }

        [TestMethod]
        public void TryToGetUser()
        {
            //string username = "admin";
            string username = "Evgeniia";

            var user = _usersCollection
                .Find(u => u.UserName == username)
                .FirstOrDefault();

            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.UserName);

            Console.WriteLine("User info:");
            Console.WriteLine($"UserName: {user.UserName}");
            Console.WriteLine($"Phone Number: {user.PhoneNumber}");
            Console.WriteLine($"Full Name: {user.FirstName} {user.LastName}");
            Console.WriteLine($"Address: {user.Address}");
            Console.WriteLine($"City: {user.City}, Country: {user.Country}");
        }

        [TestMethod]
        public void TryToRemoveHotel()
        {
            string hotelName = "Istanbul Hotel";

            var hotel = _hotelsCollection
                .Find(h => h.Name == hotelName)
                .FirstOrDefault();

            Assert.IsNotNull(hotel);

            _hotelsCollection.DeleteOne(h => h.Id == hotel.Id);

            var removedHotel = _hotelsCollection
                .Find(h => h.Name == hotelName)
                .FirstOrDefault();

            Assert.IsNull(removedHotel);
        }
    }
}
