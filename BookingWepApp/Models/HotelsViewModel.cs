using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingWepApp.Models
{
    public class HotelsViewModel
    {
        public IEnumerable<Hotel> Hotels { get; set; }
        [Required(ErrorMessage = "Введите название отеля")]
        public string SearchKeyword { get; set; }
    }
}