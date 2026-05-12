using Inventory.Core.Models;

namespace Inventory.Core.Interfaces;

public interface IMovementRepository
{
    Task AddMovementAsync(MovementHistory movement);
    Task<IEnumerable<MovementHistory>> GetByDeviceAsync(int deviceId);
}