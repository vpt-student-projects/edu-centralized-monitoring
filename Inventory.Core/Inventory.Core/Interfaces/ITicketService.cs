using Inventory.Core.Models;

namespace Inventory.Core.Interfaces;

public interface ITicketService
{
    Task<Ticket> CreateTicketAsync(int deviceId, string description, int priority = 3);
    Task<Ticket> GetTicketByIdAsync(int ticketId);
    Task<IEnumerable<Ticket>> GetOpenTicketsAsync();
    Task<IEnumerable<Ticket>> GetTicketsByRoomAsync(int roomId);
    Task CloseTicketAsync(int ticketId, string? resolution = null);
}