using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingWepApp.Models
{
    public class HotelsViewModel
    {
        public IEnumerable<Hotel> Hotels { get; set; }
        [Required(ErrorMessage = "Enter hotel name")]
        public string SearchKeyword { get; set; }
    }
}