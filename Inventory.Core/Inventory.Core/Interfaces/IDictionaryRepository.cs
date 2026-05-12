using Inventory.Core.Models;

namespace Inventory.Core.Interfaces;

public interface IDictionaryRepository
{
    Task<List<DictionaryItem>> GetByCategoryAsync(string category);
    Task<DictionaryItem?> GetByIdAsync(int id);
}