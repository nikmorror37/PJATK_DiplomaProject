using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingWepApp.Data;
using BookingWepApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace BookingWepApp.Controllers
{
    public class HomeController : Controller
    {
        //создаем экземпляр контекста
        private readonly ApplicationDbContext _appContext;

        //конструктор класса
        //инициализируем контроллер
        public HomeController(ApplicationDbContext appContext)
        {            
            _appContext = appContext;
        }

        //страница Index
        public IActionResult Index(bool clear)
        {
            //создаем новую модель
            var hotelsViewModel = new HotelsViewModel()
            {
                //кладем в нее список отелей с номерами
                Hotels = _appContext.Hotels.Include(h => h.Rooms)
            };
            //если в текущей сессии SearchKeyword не задан
            if (HttpContext.Session.GetString("SearchKeyword") == null || clear)
            {
                //то задаем ему значение пустой строки
                HttpContext.Session.SetString("SearchKeyword", string.Empty);
            }
            //открываем страницу
            return View(hotelsViewModel);
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [AutoValidateAntiforgeryToken]
        public IActionResult Search(HotelsViewModel hotelsViewModel)
        {
            //если ключевое слово поиска задано
            if (hotelsViewModel.SearchKeyword != null)
            {
                //сохраняем его в сессию
                HttpContext.Session.SetString("SearchKeyword", hotelsViewModel.SearchKeyword);
            }

            //выполняем поиск отелей по ключевому слову
            var hotelListAfterSearch = GetHotelsBySearch(
                HttpContext.Session.GetString("SearchKeyword"));

            //создаем модель
            var newHotelViewModel = new HotelsViewModel
            {
                //кладем в нее ключевое слово
                SearchKeyword = HttpContext.Session.GetString("SearchKeyword"),
                //и список найденных отелей
                Hotels = hotelListAfterSearch
            };
            //вызываем следующий метод
            return Filter(newHotelViewModel);
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        public IActionResult Filter(HotelsViewModel hotelsViewModel)
        {
            //ищем отели, если в модели hotelsViewModel список отелей пуст
            var hotelListAfterSearch = hotelsViewModel.Hotels ??
                GetHotelsBySearch(HttpContext.Session.GetString("SearchKeyword"));
            //создаем модель
            var newHotelViewModel = new HotelsViewModel
            {
                //кладем в нее ключевое слово
                SearchKeyword = HttpContext.Session.GetString("SearchKeyword"),
                //и список найденных отелей
                Hotels = hotelListAfterSearch
            };
            //возвращаем страницу Index с новым списком отелей
            return View("Index", newHotelViewModel);
        }

        //ищет отели по названию
        private IEnumerable<Hotel> GetHotelsBySearch(string hotelName)
        {
            //если имя отеля задано
            if (!string.IsNullOrEmpty(hotelName))
            {
                //создаем модель
                var hotelsViewModel = new HotelsViewModel
                {
                    //кладем в нее список отелей,
                    //у которых название содержит строку hotelName
                    Hotels = _appContext.Hotels
                        .Include(h => h.Rooms)
                        .Where(h => h.Name.Contains(hotelName))
                        .ToList()
                };
                //возвращаем список найденных отелей
                return hotelsViewModel.Hotels;
            }
            //если имя отеля не задано,
            //возвращаем список всех отелей
            return _appContext.Hotels
                .Include(h => h.Rooms)
                .ToList();
        }
    }
}