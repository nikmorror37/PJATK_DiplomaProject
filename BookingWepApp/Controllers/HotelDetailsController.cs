using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BookingWepApp.Data;
using BookingWepApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BookingWepApp.Controllers
{
    //помечаем, что использования методов этого контроллера необходимо сначала авторизоваться
    [Authorize]
    public class HotelDetailsController : Controller
    {
        //создаем экземпляр контекста
        private readonly ApplicationDbContext _appContext;
        //создаем экземпляр UserManager
        private readonly UserManager<ApplicationUser> _userManager;

        //конструктор класса
        //инициализируем контроллер
        public HotelDetailsController(ApplicationDbContext appContext,
            UserManager<ApplicationUser> userManager)
        {
            _appContext = appContext;
            _userManager = userManager;
        }

        //описываем поле Email для заполнения
        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        //здесь храним данные о текущем пользователей
        [BindProperty]
        public ApplicationUser CurrentUser { get; set; }

        //открывает страницу HotelPage, где выполняется поиск свободных номеров
        public async Task<IActionResult> HotelPage(int id)
        {
            //ищем отель по ID
            var hotel = await _appContext.Hotels
                .Where(h => h.Id == id)
                .Include(r => r.Rooms)
                .FirstOrDefaultAsync();
            //сохраняем найденный отель в статический класс
            HotelDetailsData.CurrentHotel = hotel;
            //открываем страницу
            return View(hotel);
        }

        //действие - забронировать номер
        public async Task<IActionResult> BookRoom(RoomType roomType, int id)
        {
            //сохраняем данные о текущем пользователе
            CurrentUser = await _userManager.GetUserAsync(User);
            //если пользователь найден
            if (CurrentUser != null)
            {
                //то кладем его данные в ViewBag представления
                ViewBag.User = CurrentUser;
            }
            //ищем список доступных 
            var availableRoomToBeBooked = _appContext.Rooms
                .Where(_ => _.HotelId == id && _.RoomType == roomType)
                .Include(_ => _.Hotel)
                .FirstOrDefault();

            if (availableRoomToBeBooked != null)
            {
                HttpContext.Session.SetInt32("roomId", availableRoomToBeBooked.Id);
                HttpContext.Session.SetString("CheckInDate", HotelDetailsData.CheckInDate.ToString());
                HttpContext.Session.SetString("CheckOutDate", HotelDetailsData.CheckOutDate.ToString());

                return RedirectToAction("Checkout", "Checkout");
            }
            else
            {
                return NotFound();
            }
        }

        private List<DateTime> GetStayingDaysRangeList()
        {
            var daysRangeList = new List<DateTime>();

            for (DateTime date = HotelDetailsData.CheckInDate; date <= HotelDetailsData.CheckOutDate; date = date.AddDays(1))
            {
                daysRangeList.Add(date);
            }

            return daysRangeList;
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [AutoValidateAntiforgeryToken]
        public IActionResult SearchRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            //проверяем, заданы ли даты заселения и выселения
            //(если даты не выбрать, то они автоматически задаются, как 01.01.0001)
            //поэтому, проверяем, если год у них меньше, либо равен 1, то они не заданы
            if (checkInDate.Year <= 1 || checkOutDate.Year <= 1)
            {
                //выводим ошибку
                ViewBag.Error = "Укажите даты заезда и выезда";
                //возвращаем эту же страницу
                return View("HotelPage", HotelDetailsData.CurrentHotel);
            }
            //проверяем, что дата выселения строго больше даты заселения
            if (checkOutDate <= checkInDate)
            {
                //выводим ошибку
                ViewBag.Error = "Дата выселения должна быть строго больше даты заселения";
                //возвращаем эту же страницу
                return View("HotelPage", HotelDetailsData.CurrentHotel);
            }
            //здесь мы выполняем, так называемся "сырой" SQL-запрос
            //и отправляем его напрямую при помощи класса SqlConnection
            //такой запросы было бы сложно превратить в LINQ
            //в запросе ниже мы просто ищем все номера в отеле,
            //которые свободны в заданном промежутке дат
            //даты проверяются по формуле ниже
            //Формула (StartA <= EndB) and (EndA >= StartB)
            string sql = "select q1.RoomType, q2.cnt, q1.cnt " +
                "from " +
                "(select r.RoomType, count(*) as cnt " +
                "from Hotels h " +
                "left join Rooms r on r.HotelId = h.Id " +
                $"where h.Id = {HotelDetailsData.CurrentHotel.Id} " +
                "and r.RoomType is not null " +
                "group by r.RoomType) as q1 " +
                "join " +
                "(select r.RoomType, count(*) as cnt " +
                "from Hotels h " +
                "left join Rooms r on r.HotelId = h.Id " +
                $"where h.Id = {HotelDetailsData.CurrentHotel.Id} " +
                "and r.RoomType is not null " +
                "and r.Id not in " +
                "(select b.RoomId " +
                "from Bookings b " +
                $"where ('{checkInDate.ToString("dd.MM.yyyy")}' <= b.CheckOut) and ('{checkOutDate.ToString("dd.MM.yyyy")}' >= b.CheckIn)) " +
            "group by r.RoomType) as q2 on q1.RoomType = q2.RoomType";
            //создаем подключение к БД по уже существующей строке подключения
            var connection = new SqlConnection(_appContext.Database.GetDbConnection().ConnectionString);
            //открываем подключение
            connection.Open();
            //создаем новый адаптер для заполнения временной таблицы DataTable
            var adapter = new SqlDataAdapter(sql, connection);
            //создаем временную таблицу
            var dt = new DataTable();
            //заполняем её
            adapter.Fill(dt);
            //создаем новый список номеров при помощи пользовательского класса RoomItem
            var rooms = new List<RoomItem>();
            //в цикле по строкам временной таблицы
            foreach (DataRow row in dt.Rows)
            {
                //добавляем в список rooms новый объект RoomItem
                rooms.Add(new RoomItem()
                {
                    //задаем ему тип
                    RoomType = (RoomType)row[0],
                    //число свободных номеров данного типа
                    AvailableRooms = Convert.ToInt32(row[1]),
                    //число всех номеров данного типа
                    TotalRooms = Convert.ToInt32(row[2])
                });
            }
            //кладем в ViewBag представления список свободных номеров
            ViewBag.RoomsAvailable = rooms;
            //кладем в ViewBag представления заданные даты
            ViewBag.DataInfo = new List<DateTime>() { checkInDate, checkOutDate };
            //сохраняем даты
            HotelDetailsData.CheckInDate = checkInDate;
            HotelDetailsData.CheckOutDate = checkOutDate;
            //открываем эту же страницу но уже выводим список номеров
            return View("HotelPage", HotelDetailsData.CurrentHotel);
        }
    }

    //пользовательский класс - Номер
    public class RoomItem
    {
        //тип номера
        public RoomType RoomType { get; set; }
        //число свободных номеров
        public int AvailableRooms { get; set; }
        //число всех номеров
        public int TotalRooms { get; set; }
    }
}