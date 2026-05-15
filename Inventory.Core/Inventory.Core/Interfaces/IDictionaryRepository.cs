using Inventory.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces;

public interface IDictionaryRepository
{
    Task<List<DictionaryItem>> GetByCategoryAsync(string category);
    Task<DictionaryItem?> GetByIdAsync(int id);
    Task<List<DictionaryItem>> GetAllAsync();
    Task AddAsync(DictionaryItem item);
    Task UpdateAsync(DictionaryItem item);
    Task DeleteAsync(int id);
}