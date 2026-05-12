using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Core.Models;

public class Ticket
{
    [Key]
    public int TicketID { get; set; }

    [Required]
    public int DeviceID { get; set; }

    /// <summary>
    /// Кабинет, где возникла проблема. Тикет остаётся привязан к этому кабинету даже после перемещения устройства.
    /// </summary>
    public int? RoomID { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int Priority { get; set; } = 3;                     // 1 - высокая, 2 - средняя, 3 - низкая

    [Required]
    public int StatusID { get; set; }                          // Новая / В работе / Завершена

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    [ForeignKey(nameof(DeviceID))]
    public virtual Device? Device { get; set; }

    [ForeignKey(nameof(RoomID))]
    public virtual Room? Room { get; set; }

    [ForeignKey(nameof(StatusID))]
    public virtual DictionaryItem? Status { get; set; }
}