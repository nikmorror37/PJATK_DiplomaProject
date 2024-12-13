using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BookingWepApp.Models;
using BookingWepApp.Data;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BookingWepApp.Controllers
{
    public class HomeController : Controller
    {
        // Экземпляр контекста MongoDB
        private readonly MongoDbContext _mongoContext;

        // Конструктор класса
        // Инициализируем контроллер
        public HomeController(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        // Страница Index
        public IActionResult Index(bool clear)
        {
            // Создаем новую модель
            var hotelsViewModel = new HotelsViewModel()
            {
                // Кладем в нее список отелей с номерами
                Hotels = _mongoContext.Hotels
                    .Find(_ => true)
                    .ToList()
            };

            // Если в текущей сессии SearchKeyword не задан
            if (HttpContext.Session.GetString("SearchKeyword") == null || clear)
            {
                // То задаем ему значение пустой строки
                HttpContext.Session.SetString("SearchKeyword", string.Empty);
            }

            // Открываем страницу
            return View(hotelsViewModel);
        }
        public IActionResult Manage()
        {
            var isAdmin = User.Identity != null && User.Identity.Name == "admin";
            ViewBag.IsAdmin = isAdmin; // Передаем флаг в представление
            return View();
        }


        //[HttpGet]
        //public async Task<IActionResult> AssignAdminRole()
        //{
        //	var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        //	var admin = await userManager.FindByNameAsync("admin");
        //	if (admin != null)
        //	{
        //		var result = await userManager.AddToRoleAsync(admin, "Admin");
        //		if (result.Succeeded)
        //		{
        //			return Content("Role Admin successfully assigned to user admin.");
        //		}
        //		else
        //		{
        //			return Content("Failed to assign role: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        //		}
        //	}
        //	return Content("Admin user not found.");
        //}


        // httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        // Фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [AutoValidateAntiforgeryToken]
        public IActionResult Search(HotelsViewModel hotelsViewModel)
        {
            // Если ключевое слово поиска задано
            if (hotelsViewModel.SearchKeyword != null)
            {
                // Сохраняем его в сессию
                HttpContext.Session.SetString("SearchKeyword", hotelsViewModel.SearchKeyword);
            }

            // Выполняем поиск отелей по ключевому слову
            var hotelListAfterSearch = GetHotelsBySearch(
                HttpContext.Session.GetString("SearchKeyword"));

            // Создаем модель
            var newHotelViewModel = new HotelsViewModel
            {
                // Кладем в нее ключевое слово
                SearchKeyword = HttpContext.Session.GetString("SearchKeyword"),
                // И список найденных отелей
                Hotels = hotelListAfterSearch
            };

            // Вызываем следующий метод
            return Filter(newHotelViewModel);
        }

        // httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        public IActionResult Filter(HotelsViewModel hotelsViewModel)
        {
            // Ищем отели, если в модели hotelsViewModel список отелей пуст
            var hotelListAfterSearch = hotelsViewModel.Hotels ??
                GetHotelsBySearch(HttpContext.Session.GetString("SearchKeyword"));

            // Создаем модель
            var newHotelViewModel = new HotelsViewModel
            {
                // Кладем в нее ключевое слово
                SearchKeyword = HttpContext.Session.GetString("SearchKeyword"),
                // И список найденных отелей
                Hotels = hotelListAfterSearch
            };

            // Возвращаем страницу Index с новым списком отелей
            return View("Index", newHotelViewModel);
        }

        // Ищет отели по названию
        private IEnumerable<Hotel> GetHotelsBySearch(string hotelName)
        {
            // Если имя отеля задано
            if (!string.IsNullOrEmpty(hotelName))
            {
                // Ищем отели, у которых название содержит строку hotelName
                var hotels = _mongoContext.Hotels
                    .Find(h => h.Name.ToLower().Contains(hotelName.ToLower()))
                    .ToList();

                return hotels;
            }

            // Если имя отеля не задано, возвращаем список всех отелей
            return _mongoContext.Hotels
                .Find(_ => true)
                .ToList();
        }
    }
}
