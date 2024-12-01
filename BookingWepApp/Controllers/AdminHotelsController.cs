using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingWepApp.Data;
using BookingWepApp.Models;

namespace BookingWepApp.Controllers
{
    //данная строка позволяет установить права доступа
    //здесь мы помечаем, что доступ ко всем методам этого контроллера имеет только Admin
    [Authorize(Roles = "Admin")]
    public class AdminHotelsController : Controller
    {
        //создаем экземпляр контекста
        private readonly ApplicationDbContext _context;
        //предоставляет сведения о среде веб-размещения
        //будем использовать для получения доступа к папке wwwroot,
        //где хранятся различные данные, вроде изображений
        private readonly IWebHostEnvironment _webHostEnvironment;

        //конструктор класса
        //инициализируем контроллер
        public AdminHotelsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        //открывает страницу Index
        //в контроллерах имена методов должны совпадать с именами представлений
        public async Task<IActionResult> Index()
        {
            //октрывается страница Index
            //передаем в нее список Отелей из БД
            return View(await _context.Hotels.ToListAsync());
        }

        //октрывает страницу Create
        public IActionResult Create()
        {
            //октрывается страница Create
            //ничего не передаем в нее, просто открываем страницу

            return View();
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HotelId,Name,Website,Address,ZipCode,City,Country,Description,Stars,DistanceFromCenter,ImageUrl,RatedByGuests")] Hotel hotel)
        {
            //если после выполнения операции ошибок не возникло
            if (ModelState.IsValid)
            {
                //получаем путь до папки wwwroot
                var webRootPath = _webHostEnvironment.WebRootPath;
                //получаем файлы, которые передали из формы
                //(изображение выбирается из выпадающего списка)
                var files = HttpContext.Request.Form.Files;
                //создаем новое случайное имя для файла при помощи GUID
                var fileName = Guid.NewGuid().ToString();
                //формируем путь
                var folder = Path.Combine(webRootPath, @"pictures/hotels");
                //получаем расширение файла
                var extension = Path.GetExtension(files[0].FileName);
                //при помощи потока FileStream создаем новый файл
                using (var fileStream = new FileStream(Path.Combine(folder, fileName + extension), FileMode.Create))
                {
                    //записываем в него данные
                    await files[0].CopyToAsync(fileStream);
                }
                //назначаем URL картинки отелю
                hotel.ImageUrl = @"~/pictures/hotels/" + fileName + extension;
                //говорим контексту, что мы добавили данные
                _context.Add(hotel);
                //сохраняем все изменения
                await _context.SaveChangesAsync();
                //переводим на страницу Index
                return RedirectToAction(nameof(Index));
            }
            //если ошибки все же были
            //то остаемся на этой же странице
            return View(hotel);
        }

        //переводит на страницу Edit
        //но при этом, выполняет поиск объекта в БД по заданному ID
        public async Task<IActionResult> Edit(int? id)
        {
            //если ID не задан
            if (id == null)
            {
                //возвращаем страницу 404
                return NotFound();
            }
            //иначе, ищем отель в БД по ID
            var hotel = await _context.Hotels.FindAsync(id);
            //если отель не найден
            if (hotel == null)
            {
                //возвращаем страницу 404
                return NotFound();
            }
            //переходим на страницу Edit
            return View(hotel);
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HotelId,Name,Website,Address,ZipCode,City,Country,Description,Stars,DistanceFromCenter,ImageUrl,RatedByGuests")] Hotel hotel)
        {
            //если после выполнения операции ошибок не возникло
            if (ModelState.IsValid)
            {
                //в блоке try catch отлавливаем исключения, чтобы приложение не вылетело
                try
                {
                    //ищем отель
                    var item = _context.Hotels.Find(id);
                    //получаем путь до папки wwwroot
                    var webRootPath = _webHostEnvironment.WebRootPath;
                    //получаем файлы, которые передали из формы
                    //(изображение выбирается из выпадающего списка)
                    var files = HttpContext.Request.Form.Files;
                    //если количество файлов больше 0
                    if (files.Count > 0)
                    {
                        //создаем новое случайное имя для файла при помощи GUID
                        var fileName = Guid.NewGuid().ToString();
                        //формируем путь
                        var folder = Path.Combine(webRootPath, @"pictures/hotels");
                        //получаем расширение файла
                        var extension = Path.GetExtension(files[0].FileName);
                        //при помощи потока FileStream создаем новый файл
                        using (var fileStream = new FileStream(Path.Combine(folder, fileName + extension), FileMode.Create))
                        {
                            //записываем в него данные
                            await files[0].CopyToAsync(fileStream);
                        }
                        //назначаем URL картинки отелю
                        item.ImageUrl = @"~/pictures/hotels/" + fileName + extension;
                    }
                    //иначе, если файлы не найдены
                    else
                    {
                        //то для URL картинки отеля задаем ему тот же URL, что был до этого при добавлении
                        item.ImageUrl = _context.Hotels.Where(h => h.Id == hotel.Id)
                            .Select(h => h.ImageUrl)
                            .FirstOrDefault();
                    }
                    //переписываем все остальные поля
                    //в соответствии с данными, которые заданы на странице
                    item.Stars = hotel.Stars;
                    item.Address = hotel.Address;
                    item.City = hotel.City;
                    item.Country = hotel.Country;
                    item.Website = hotel.Website;
                    item.DistanceFromCenter = hotel.DistanceFromCenter;
                    item.Description = hotel.Description;
                    item.ZipCode = hotel.ZipCode;
                    item.Name = hotel.Name;
                    //сохраняем
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    //если получили исключение
                    if (!HotelExists(hotel.Id))
                    {
                        //выводим 404
                        return NotFound();
                    }
                    else
                    {
                        //неизвестная ошибка
                        throw;
                    }
                }
                //переводим на страницу Index
                return RedirectToAction(nameof(Index));
            }
            //если ошибки все же были
            //то остаемся на этой же странице
            return View(hotel);
        }

        //переводит на страницу Delete
        //но при этом, выполняет поиск объекта в БД по заданному ID
        public async Task<IActionResult> Delete(int? id)
        {
            //если ID не задан
            if (id == null)
            {
                //возвращаем страницу 404
                return NotFound();
            }
            //ищем отель по ID
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(m => m.Id == id);
            //если отель не найден
            if (hotel == null)
            {
                //возвращаем страницу 404
                return NotFound();
            }

            //иначе, переходим на страницу Delete
            return View(hotel);
        }

        //httpPost помечает метод, который предназначен для передачи данных
        //говорим, что этот метод удаляет объект
        [HttpPost, ActionName("Delete")]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //ищем отель по ID
            var hotel = await _context.Hotels.FindAsync(id);
            //удаляем отель из БД
            _context.Hotels.Remove(hotel);
            //сохраняем
            await _context.SaveChangesAsync();
            //переводим на страницу Index
            return RedirectToAction(nameof(Index));
        }

        //метод для проверки существования отеля
        private bool HotelExists(int id)
        {
            //если существует любой объект с заданным ID
            //то возвращаем True, иначе, False
            return _context.Hotels.Any(e => e.Id == id);
        }
    }
}