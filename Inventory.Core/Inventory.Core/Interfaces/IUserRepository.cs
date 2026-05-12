using Inventory.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login, string passwordHash);
    Task<User?> GetByIdAsync(int userId);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);              // новый
    Task UpdateAsync(User user);           // новый
    Task DeleteAsync(int userId);          // новый
}