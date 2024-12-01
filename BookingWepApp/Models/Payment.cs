using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingWepApp.Models
{
    /// <summary>
    /// Оплата
    /// </summary>
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public Status Status { get; set; }
        public DateTime Date { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }
        [Required]
        [Display(Name = "Тип кредитной карты")]
        public CardType Type { get; set; }
        [Required]
        [Display(Name = "Номер кредитной карты")]
        [RegularExpression(@"^([0-9]{16})$", ErrorMessage = "Введите 16-значный номер кредитной карты")]
        public string CardNumber { get; set; }
        [Required]
        [RegularExpression(@"^([0-9]{3})$", ErrorMessage = "Код CVV указан не верно")]
        public string CVV { get; set; }
        [Required]
        [Display(Name = "Имя на кредитной карте")]
        public string CardHolderFirstName { get; set; }
        [Required]
        [Display(Name = "Фамилия на кредитной карте")]
        public string CardHolderLastName { get; set; }
        [Required]
        [Display(Name = "Срок действия")]
        public string ExpirationDate { get; set; }
        public Booking Booking { get; set; }
    }

    public enum CardType { Visa, MasterCard }
}