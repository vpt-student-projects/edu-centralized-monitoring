using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.Models;

public class DictionaryItem
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;   // DeviceType, DeviceStatus, TicketStatus

    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;      // "Стандартный ПК", "Сломан", "Новая"
}