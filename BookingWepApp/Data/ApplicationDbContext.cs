using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookingWepApp.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace BookingWepApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private SqlConnectionStringBuilder connectionStringBuilder;

        public ApplicationDbContext()
        {
            connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = "DESKTOP-HFN3ITE";
            connectionStringBuilder.InitialCatalog = "BookingOne";
            connectionStringBuilder.IntegratedSecurity = true;

            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionStringBuilder.ToString());
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Hotel>()
                .HasMany(h => h.Rooms)
                .WithOne(r => r.Hotel);
            builder.Entity<HotelsViewModel>()
                .HasNoKey();

            builder.Entity<Hotel>()
                .HasData(new Hotel
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
                });
            builder.Entity<Hotel>()
                .HasData(new Hotel
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
                });
            builder.Entity<Hotel>()
                .HasData(new Hotel
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
                });

            Random random = new Random();
            int totalNumberOfRooms = 100;
            var roomsList = new List<Room>();
            var numberOfHotels = 3;
            var roomDescriptionArray = new[]
            {
                    "Традиционно оформленный номер с телевизором, принадлежностями для чая/кофе и собственной душевой." ,
                    "Двухместный номер с 1 кроватью, кондиционером, телевизором с плоским экраном, телефоном с прямым набором номера, высокоскоростным Wi-Fi, феном и сейфом. В современной ванной комнате установлен душ с сильным напором воды.",
                    "В этом номере есть спутниковое телевидение, принадлежности для чая/кофе, фен и собственная ванная комната.",
                    "Трехместный номер с кондиционером, электрическим чайником, бесплатным Wi-Fi и феном."
                };
            var numberOfBedsArray = new[] { 1, 1, 2, 2 };
            var capacityArray = new[] { 1, 2, 2, 3 };
            var roomImagesArray = new[]
            {
                    "~/pictures/rooms/single.jpg",
                    "~/pictures/rooms/double.jpg",
                    "~/pictures/rooms/twin.jpg",
                    "~/pictures/rooms/triple.jpg",
                };

            for (int i = 0; i < totalNumberOfRooms; i++)
            {
                var index = random.Next(Enum.GetNames(typeof(RoomType)).Length);
                var randomHotel = random.Next(1, numberOfHotels + 1);

                builder.Entity<Room>()
                    .HasData(new Room
                    {
                        Id = i + 1,
                        RoomType = (RoomType)index,
                        RoomPrice = ((index + 1) * 100) + ((randomHotel + 1) * 50),
                        RoomDescription = roomDescriptionArray[index],
                        NumberOfBeds = numberOfBedsArray[index],
                        Capacity = capacityArray[index],
                        RoomImageUrl = roomImagesArray[index],
                        HotelId = randomHotel
                    });
            }
        }
    }
}