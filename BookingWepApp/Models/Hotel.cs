using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Отель
    /// </summary>
    public class Hotel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Название отеля")]
        [StringLength(30)]
        public string Name { get; set; }
        [Required]
        [Url]
        [DisplayName("Ссылка на сайт")]
        public string Website { get; set; }
        [Required]
        [Display(Name = "Адрес")]
        public string Address { get; set; }
        [Required]
        [RegularExpression("^[a-zA-Z0-9- ]*$", ErrorMessage = "Допустимы только буквы, цифры, дефис (-) и пробел.")]
        [Display(Name = "Индекс")]
        public string ZipCode { get; set; }
        [Required]
        [Display(Name = "Город")]
        public string City { get; set; }
        [Required]
        [Display(Name = "Страна")]
        public string Country { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Описание (макс. 255 символов)")]
        public string Description { get; set; }
        [Required]
        [Range(1, 5)]
        [Display(Name = "Рейтинг")]
        public int Stars { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Расстояние до центра")]
        public double DistanceFromCenter { get; set; }
        [DisplayName("Фотография отеля")]
        public string ImageUrl { get; set; }
        public ICollection<Room> Rooms { get; set; }
    }
}