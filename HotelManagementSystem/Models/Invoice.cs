using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models
{
    public class Invoice
    {
        [Key] public int InvoiceId { get; set; }
        public int ReservationId { get; set; }
        public DateTime InvoiceDate { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
    }
}