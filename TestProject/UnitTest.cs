using BookingWepApp.Data;
using BookingWepApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TryToGetHotels()
        {
            using (var db = new ApplicationDbContext())
            {
                var list = db.Hotels.ToList();
                Assert.IsNotNull(list);
            }
        }

        [TestMethod]
        public void TryToCreateHotel()
        {
            bool error = false;

            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var hotel = new Hotel()
                    {
                        Address = "ул. Такая-то, 12",
                        City = "Стамбул",
                        Country = "Турция",
                        Description = "Description",
                        DistanceFromCenter = 10,
                        Name = "Стамбул Hotel",
                        Stars = 4,
                        ZipCode = "238493",
                        Website = "https://hotel-best.tr"
                    };
                    db.Hotels.Add(hotel);
                    db.SaveChanges();
                }
            }
            catch
            {
                error = true;
            }

            Assert.IsFalse(error);
        }

        [TestMethod]
        public void TryToGetRoom()
        {
            using (var db = new ApplicationDbContext())
            {
                var room = db.Rooms.Find(1);
                Assert.IsNotNull(room);
            }
        }

        [TestMethod]
        public void TryToGetUser()
        {
            string username = "admin";

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Where(u => u.UserName == username).FirstOrDefault();
                Assert.AreEqual(username, user.UserName);
            }
        }

        [TestMethod]
        public void TryToRemoveHotel()
        {
            string hotelName = "Стамбул Hotel";

            using (var db = new ApplicationDbContext())
            {
                var hotel = db.Hotels.Where(_ => _.Name == hotelName).FirstOrDefault();
                db.Hotels.Remove(hotel);
                db.SaveChanges();

                var removedHotel = db.Hotels.Where(_ => _.Name == hotelName).FirstOrDefault();
                Assert.IsNull(removedHotel);
            }
        }
    }
}