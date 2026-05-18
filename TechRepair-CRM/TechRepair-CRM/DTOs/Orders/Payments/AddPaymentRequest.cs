using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders.Payments;

public record AddPaymentRequest
{
    [Required(ErrorMessage = "Сумма обязательна.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сумма оплаты должна быть больше 0.")]
    [Display(Name = "Сумма")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Выберите способ оплаты.")]
    [Display(Name = "Способ оплаты")]
    public string PaymentMethod { get; set; } = "Cash";

    [Display(Name = "Номер транзакции")]
    public string? TransactionNumber { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}