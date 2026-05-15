using Inventory.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces;

public interface IRoomRepository
{
    Task<List<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(int roomId);
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int roomId);
}