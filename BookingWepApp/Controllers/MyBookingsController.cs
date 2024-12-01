using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingWepApp.Data;
using BookingWepApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace BookingWepApp.Controllers
{
    //помечаем, что использования методов этого контроллера необходимо сначала авторизоваться
    [Authorize]
    public class MyBookingsController : Controller
    {
        //создаем экземпляр контекста
        private readonly ApplicationDbContext _appContext;
        //создаем экземпляр UserManager
        private readonly UserManager<ApplicationUser> _userManager;
        //текущий пользователь системы
        [BindProperty]
        public ApplicationUser CurrentUser { get; set; }

        //конструктор класса
        //инициализируем контроллер
        public MyBookingsController(ApplicationDbContext appContext, UserManager<ApplicationUser> userManager)
        {
            _appContext = appContext;
            _userManager = userManager;
        }

        //открывает страницу MyBookings
        public async Task<IActionResult> MyBookings()
        {
            //получаем текущего, авторизованного пользователя
            CurrentUser = await _userManager.GetUserAsync(User);
            //если он найден
            if (CurrentUser != null)
            {
                //передаем его в ViewBag представления
                //ViewBag - объект, предоставляющий динамический доступ к объектам
                ViewBag.User = CurrentUser;
            }
            //получаем список бронирований пользователя
            //у которых статус равен Accepted
            var myBookingsList = await _appContext.Bookings
                                    .Where(b => b.UserId == CurrentUser.Id && b.Status == Status.Accepted)
                                    .Include(b => b.Payment)
                                    .Where(p => p.Status == Status.Accepted)
                                    .Include(b => b.Room)
                                    .ThenInclude(r => r.Hotel)
                                    .ToListAsync();
            //открываем страницу
            return View(myBookingsList);
        }
    }
}