using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Комната
    /// </summary>
    public class Room
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Тип номера")]
        public RoomType RoomType { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        [Range(0, double.PositiveInfinity)]
        [Display(Name = "Стоимость за ночь")]
        public decimal RoomPrice { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Описание (макс. 255 символов)")]
        public string RoomDescription { get; set; }
        [Required]
        [Display(Name = "Количество кроватей")]
        [Range(1, 5)]
        public int NumberOfBeds { get; set; }
        [Required]
        [Display(Name = "Вместимость/количество человек")]
        [Range(1, 7)]
        public int Capacity { get; set; }
        [DisplayName("Фотография номера")]
        public string RoomImageUrl { get; set; }
        [Required]
        [DisplayName("Отель")]
        public int HotelId { get; set; }
        [DisplayName("Отель")]
        public Hotel Hotel { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }

    public enum RoomType { Single, Double, Twin, Triple }
}