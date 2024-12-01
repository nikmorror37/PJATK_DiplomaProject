using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingWepApp.Data;
using BookingWepApp.Models;

namespace BookingWepApp.Controllers
{
    //данная строка позволяет установить права доступа
    //здесь мы помечаем, что доступ ко всем методам этого контроллера имеет только Admin
    [Authorize(Roles = "Admin")]
    public class AdminRoomsController : Controller
    {
        //создаем экземпляр контекста
        private readonly ApplicationDbContext _context;

        //конструктор класса
        //инициализируем контроллер
        public AdminRoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //открывает страницу Index
        //но здесь передаем в качестве параметра ID номера
        public async Task<IActionResult> Index(int? id)
        {
            //создаем список всех номеров
            var applicationDbContext = _context.Rooms.Include(r => r.Hotel);
            //если ID задан
            if (id != null)
            {
                //то фильтруем список номеров по ID
                applicationDbContext = _context.Rooms.Where(r => r.Id == id).Include(r => r.Hotel);
            }
            //открываем страницу и передаем в нее список номеров
            return View(await applicationDbContext.ToListAsync());
        }

        //октрывает страницу Create
        public IActionResult Create()
        {
            //передаем в данные представления список отелей для привязки к выпадающему списку
            ViewData["HotelId"] = new SelectList(_context.Hotels.ToList(), "Id", "Name");
            //открываем страницу
            return View();
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomType,RoomNumber,RoomPrice,RoomDescription,NumberOfBeds,Capacity,IsAvailable,RoomImageUrl,HotelId")] Room room)
        {
            //если после выполнения операции ошибок не возникло
            if (ModelState.IsValid)
            {
                //говорим контексту, что мы добавили данные
                _context.Add(room);
                //сохраняем все изменения
                await _context.SaveChangesAsync();
                //переводим на страницу Index
                return RedirectToAction(nameof(Index));
            }
            //если ошибки все же были
            //в любом случае передаем список отелей для привязки к выпадающему списку
            ViewData["HotelId"] = new SelectList(_context.Hotels.ToList(), "Id", "Name", room.HotelId);
            //но остаемся на этой же странице
            return View(room);
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
            //иначе, ищем номер в БД по ID
            var room = await _context.Rooms.FindAsync(id);
            //если номер не найден
            if (room == null)
            {
                //возвращаем страницу 404
                return NotFound();
            }
            //передаем список отелей для привязки к выпадающему списку
            ViewData["HotelId"] = new SelectList(_context.Hotels.ToList(), "Id", "Name", room.HotelId);
            //переходим на страницу Edit
            return View(room);
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomType,RoomNumber,RoomPrice,RoomDescription,NumberOfBeds,Capacity,IsAvailable,RoomImageUrl,HotelId")] Room room)
        {
            //если после выполнения операции ошибок не возникло
            if (ModelState.IsValid)
            {
                //ищем номер по ID
                var item = _context.Rooms.Find(id);
                //передаем в него новые данные со страницы
                item.NumberOfBeds = room.NumberOfBeds;
                item.RoomPrice = room.RoomPrice;
                item.RoomDescription = room.RoomDescription;
                item.Capacity = room.Capacity;
                item.HotelId = room.HotelId;
                item.RoomType = room.RoomType;
                item.RoomImageUrl = room.RoomImageUrl;
                //сохраняем
                await _context.SaveChangesAsync();
                //переводим на страницу Index
                return RedirectToAction(nameof(Index));
            }
            //в любом случае передаем список отелей для привязки к выпадающему списку
            ViewData["HotelId"] = new SelectList(_context.Hotels.ToList(), "Id", "Name", room.HotelId);
            //остаемся на текущей странице
            return View(room);
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
            //ищем номер по ID
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(m => m.Id == id);
            //если номер не найден
            if (room == null)
            {
                //возвращаем страницу 404
                return NotFound();
            }
            //иначе, переходим на страницу Delete
            return View(room);
        }

        //httpPost помечает метод, который предназначен для передачи данных
        //говорим, что этот метод удаляет объект
        [HttpPost, ActionName("Delete")]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //ищем номер по ID
            var room = await _context.Rooms.FindAsync(id);
            //удаляем номер из БД
            _context.Rooms.Remove(room);
            //сохраняем
            await _context.SaveChangesAsync();
            //переводим на страницу Index
            return RedirectToAction(nameof(Index));
        }

        //метод для проверки существования номера
        private bool RoomExists(int id)
        {
            //если существует любой объект с заданным ID
            //то возвращаем True, иначе, False
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}