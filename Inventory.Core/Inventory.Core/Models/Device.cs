using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Core.Models;

public class Device
{
    [Key]
    public int DeviceID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;           // gk_220_1

    [Required]
    public int TypeID { get; set; }

    [MaxLength(1000)]
    public string? Specs { get; set; }

    [Required]
    public int StatusID { get; set; }

    [Required]
    public int CurrentRoomID { get; set; }

    /// <summary>
    /// Позиция плитки внутри кабинета (1, 2, 3...). 
    /// Используется для правильного отображения в WPF
    /// </summary>
    public int? PositionInRoom { get; set; }

    public int? AssignedToUserID { get; set; }

    // Навигационные свойства
    [ForeignKey(nameof(CurrentRoomID))]
    public virtual Room? CurrentRoom { get; set; }

    [ForeignKey(nameof(TypeID))]
    public virtual DictionaryItem? Type { get; set; }

    [ForeignKey(nameof(StatusID))]
    public virtual DictionaryItem? Status { get; set; }

    [ForeignKey(nameof(AssignedToUserID))]
    public virtual User? AssignedTo { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public virtual ICollection<MovementHistory> MovementHistory { get; set; } = new List<MovementHistory>();
}