using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BookingWepApp.Models;
using Microsoft.AspNetCore.Hosting;

namespace BookingWepApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminHotelsController : Controller
    {
        private readonly IMongoCollection<Hotel> _hotelsCollection;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminHotelsController(IMongoClient mongoClient, IWebHostEnvironment webHostEnvironment)
        {
            var database = mongoClient.GetDatabase("BookingDb");
            _hotelsCollection = database.GetCollection<Hotel>("Hotels");
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: AdminHotels
        public async Task<IActionResult> Index()
        {
            var hotels = await _hotelsCollection.Find(_ => true).ToListAsync();
            return View(hotels);
        }

        // GET: AdminHotels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminHotels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Website,Address,ZipCode,City,Country,Description,Stars,DistanceFromCenter,ImageUrl")] Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var folder = Path.Combine(webRootPath, "pictures/hotels");
                    var extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(folder, fileName + extension), FileMode.Create))
                    {
                        await files[0].CopyToAsync(fileStream);
                    }

                    hotel.ImageUrl = $"/pictures/hotels/{fileName}{extension}";
                }

                await _hotelsCollection.InsertOneAsync(hotel);
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);
        }

        // GET: AdminHotels/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var hotel = await _hotelsCollection.Find(h => h.Id == id).FirstOrDefaultAsync();
            if (hotel == null)
                return NotFound();

            return View(hotel);
        }

        // POST: AdminHotels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Website,Address,ZipCode,City,Country,Description,Stars,DistanceFromCenter,ImageUrl")] Hotel hotel)
        {
            if (id != hotel.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var webRootPath = _webHostEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;

                    if (files.Count > 0)
                    {
                        var fileName = Guid.NewGuid().ToString();
                        var folder = Path.Combine(webRootPath, "pictures/hotels");
                        var extension = Path.GetExtension(files[0].FileName);

                        using (var fileStream = new FileStream(Path.Combine(folder, fileName + extension), FileMode.Create))
                        {
                            await files[0].CopyToAsync(fileStream);
                        }

                        hotel.ImageUrl = $"/pictures/hotels/{fileName}{extension}";
                    }

                    var filter = Builders<Hotel>.Filter.Eq(h => h.Id, id);
                    await _hotelsCollection.ReplaceOneAsync(filter, hotel);
                }
                catch
                {
                    if (!(await HotelExists(hotel.Id)))
                        return NotFound();

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);
        }

        // GET: AdminHotels/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var hotel = await _hotelsCollection.Find(h => h.Id == id).FirstOrDefaultAsync();
            if (hotel == null)
                return NotFound();

            return View(hotel);
        }

        // POST: AdminHotels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _hotelsCollection.DeleteOneAsync(h => h.Id == id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> HotelExists(string id)
        {
            var count = await _hotelsCollection.CountDocumentsAsync(h => h.Id == id);
            return count > 0;
        }
    }
}
