using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Core.Models;

public class MovementHistory
{
    [Key]
    public int HistoryID { get; set; }

    [Required]
    public int DeviceID { get; set; }

    [Required]
    public int OldRoomID { get; set; }

    [Required]
    public int NewRoomID { get; set; }

    public DateTime MoveDate { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? Reason { get; set; }

    [ForeignKey(nameof(DeviceID))]
    public virtual Device? Device { get; set; }

    [ForeignKey(nameof(OldRoomID))]
    public virtual Room? OldRoom { get; set; }

    [ForeignKey(nameof(NewRoomID))]
    public virtual Room? NewRoom { get; set; }
}