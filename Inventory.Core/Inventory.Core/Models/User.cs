using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.Models;

public class User
{
    [Key]
    public int UserID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Login { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(20)]
    public string Role { get; set; } = "User";   // Admin, Teacher и т.д.
}