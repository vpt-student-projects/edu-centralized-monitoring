using Inventory.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket);
    Task<Ticket?> GetByIdAsync(int ticketId);
    Task<IEnumerable<Ticket>> GetOpenTicketsAsync();
    Task<IEnumerable<Ticket>> GetTicketsByRoomAsync(int roomId);
    Task UpdateAsync(Ticket ticket);
    Task<IEnumerable<Ticket>> GetByDeviceAsync(int deviceId);
    Task<IEnumerable<Ticket>> GetAllTicketsAsync(); // новый метод
}