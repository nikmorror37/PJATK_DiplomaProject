using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BookingWepApp.Data;
using BookingWepApp.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingWepApp.Controllers
{
    //помечаем, что использования методов этого контроллера необходимо сначала авторизоваться
    [Authorize]
    public class CheckoutController : Controller
    {
        //создаем экземпляр контекста
        private readonly ApplicationDbContext _appContext;
        //создаем экземпляр UserManager
        private readonly UserManager<ApplicationUser> _userManager;

        //конструктор класса
        //инициализируем контроллер
        public CheckoutController(ApplicationDbContext appContext,
            UserManager<ApplicationUser> userManager)
        {
            _appContext = appContext;
            _userManager = userManager;
        }

        //открываем страницу Checkout (оплата)
        public async Task<IActionResult> Checkout()
        {
            //берем данные авторизованного пользователя
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //получаем удостоверение пользователя по имени
            var identityClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //создаем новое бронирование
            var booking = new Booking()
            {
                //задаем все поля со страницы
                RoomId = Convert.ToInt32(HttpContext.Session.GetInt32("roomId")),
                CheckIn = Convert.ToDateTime((HttpContext.Session.GetString("CheckInDate"))),
                CheckOut = Convert.ToDateTime((HttpContext.Session.GetString("CheckOutDate"))),
                Status = Status.Pending,
                UserId = identityClaim.Value
            };
            //добавляем бронирование в БД
            _appContext.Bookings.Add(booking);
            //сохраняем
            await _appContext.SaveChangesAsync();
            //кладем в ViewBag представления данные о стоимости номера
            ViewBag.RoomPrice = decimal.Round(
                _appContext.Rooms
                .Where(r => r.Id == booking.RoomId)
                .Select(r => r.RoomPrice)
                .FirstOrDefault(), 2, MidpointRounding.AwayFromZero
                );
            //кладем в ViewBag представления данные о количество ночей
            ViewBag.NumberOfNights = Convert.ToDecimal((booking.CheckOut - booking.CheckIn).TotalDays);
            //кладем в ViewBag представления данные об итоговой стоимости бронирования
            ViewBag.TotalPrice = decimal.Round(ViewBag.RoomPrice * ViewBag.NumberOfNights, 2, MidpointRounding.AwayFromZero);
            //сохраняем ID бронирования в рамках текущей пользовательской сессии
            HttpContext.Session.SetInt32("bookingId", booking.Id);
            //открываем страницу
            return View();
        }

        //httpPost помечает метод, который предназначен для передачи данных
        [HttpPost]
        //фильтр ValidateAntiForgeryToken предназначен для противодействия подделке межсайтовых запросов
        [AutoValidateAntiforgeryToken]
        public IActionResult Checkout(Payment payment)
        {
            //если после выполнения операции ошибок не возникло
            if (ModelState.IsValid)
            {
                //создаем новую оплату
                var newPayment = new Payment()
                {
                    //задаем ей данные со страницы
                    Status = Status.Pending,
                    Date = DateTime.Now.Date,
                    Amount = payment.Amount,
                    Type = payment.Type,
                    CardNumber = payment.CardNumber.Substring(12),
                    CVV = payment.CVV.Substring(0, 1),
                    ExpirationDate = payment.ExpirationDate,
                    CardHolderFirstName = payment.CardHolderFirstName,
                    CardHolderLastName = payment.CardHolderLastName
                };
                //добавляем оплату в БД
                _appContext.Payments.Add(newPayment);
                //сохраняем
                _appContext.SaveChanges();
                //переводим на действие PaymentCheckout
                return RedirectToAction("PaymentCheckout", newPayment);
            }
            else
            {
                //если возникла ошибка, то остаемся на этой же странице
                return View();
            }
        }

        public IActionResult PaymentCheckout(Payment payment)
        {
            //если оплата существует
            if (payment != null)
            {
                //получаем бронирование из БД по ID, который сохранили ранее
                var bookingFromDb = _appContext.Bookings
                    .FirstOrDefault(b => b.Id == HttpContext.Session.GetInt32("bookingId"));
                //задаем статус бронированию
                bookingFromDb.Status = Status.Accepted;
                //указываем бронированию ID оплаты
                bookingFromDb.PaymentId = payment.Id;
                //сохраняем изменения
                _appContext.Update(bookingFromDb);
                //получаем оплату из БД
                var paymentFromDb = _appContext.Payments.FirstOrDefault(p => p == payment);
                //задаем ей статус
                paymentFromDb.Status = Status.Accepted;
                //сохраняем изменения
                _appContext.Update(paymentFromDb);
                //сохраняем БД
                _appContext.SaveChanges();
                //переводим на страницу BookingConfirmation
                return RedirectToAction("BookingConfirmation", paymentFromDb);
            }
            //иначе, остаемся на этой же странице
            return View();
        }

        //открываем страницу BookingConfirmation
        public IActionResult BookingConfirmation(Payment payment)
        {
            //берем данные авторизованного пользователя
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //получаем удостоверение пользователя по имени
            var identityClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //ищем самого пользователя
            var user = _appContext.ApplicationUsers.FirstOrDefault(u => u.Id == identityClaim.Value);
            //получаем ID бронирования, который сохранили ранее
            var bookingId = HttpContext.Session.GetInt32("bookingId");
            //получаем само бронирование
            var booking = _appContext.Bookings.FirstOrDefault(b => b.Id == bookingId);
            //ищем номер
            var room = _appContext.Rooms.FirstOrDefault(r => r.Id == booking.RoomId);
            //ищем отель
            var hotel = _appContext.Hotels.FirstOrDefault(h => h.Id == room.HotelId);
            //задаем для модели соответствующие данные
            var bookingConfVM = new BookingConfirmationViewModel()
            {
                User = user,
                Payment = payment,
                Booking = booking,
                Room = room,
                Hotel = hotel,
            };
            //открываем страницу подтверждения
            return View(bookingConfVM);
        }
    }
}