using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders.Payments;

public record AddPaymentRequest
{
    [Required]
    [Range(0.01, double.MaxValue)]
    [Display(Name = "Сумма")]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Способ оплаты")]
    public string PaymentMethod { get; set; } = "Cash";

    [Display(Name = "Номер транзакции")]
    public string? TransactionNumber { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}