using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using Inventory.Core.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IDeviceService _deviceService;

    public const int NewStatusId = 5;
    public const int InProgressStatusId = 6;
    public const int ClosedStatusId = 7;
    public const int BrokenStatusId = 4;
    public const int WorkingStatusId = 3;

    public TicketService(ITicketRepository ticketRepository, IDeviceService deviceService)
    {
        _ticketRepository = ticketRepository;
        _deviceService = deviceService;
    }

    public async Task<Ticket> CreateTicketAsync(int deviceId, string description, int priority = 3)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException("Описание заявки обязательно");

        var device = await _deviceService.GetDeviceByIdAsync(deviceId);
        if (device == null)
            throw new BusinessException($"Устройство с ID {deviceId} не найдено");

        var ticket = new Ticket
        {
            DeviceID = deviceId,
            RoomID = device.CurrentRoomID,
            Description = description.Trim(),
            Priority = priority,
            StatusID = NewStatusId,
            CreatedAt = DateTime.UtcNow
        };

        await _ticketRepository.AddAsync(ticket);
        await _deviceService.UpdateDeviceStatusAsync(deviceId, BrokenStatusId);
        return ticket;
    }

    public async Task CloseTicketAsync(int ticketId, string? resolution = null)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
            throw new BusinessException($"Заявка с ID {ticketId} не найдена");

        ticket.StatusID = ClosedStatusId;
        ticket.ClosedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(resolution))
            ticket.Description += $"\n\nРешение: {resolution}";

        await _ticketRepository.UpdateAsync(ticket);
    }

    public Task<Ticket> GetTicketByIdAsync(int ticketId)
        => _ticketRepository.GetByIdAsync(ticketId);

    public Task<IEnumerable<Ticket>> GetOpenTicketsAsync()
        => _ticketRepository.GetOpenTicketsAsync();

    public Task<IEnumerable<Ticket>> GetTicketsByRoomAsync(int roomId)
        => _ticketRepository.GetTicketsByRoomAsync(roomId);
}