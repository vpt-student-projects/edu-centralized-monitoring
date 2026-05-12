using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.Models;

public class Room
{
    [Key]
    public int RoomID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;   // "220", "216" и т.д.

    public int Floor { get; set; }
    public string Building { get; set; } = string.Empty;   // было int
    public string? Description { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}