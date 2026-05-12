using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using Inventory.Core.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMovementRepository _movementRepository;

    public DeviceService(IDeviceRepository deviceRepository, IMovementRepository movementRepository)
    {
        _deviceRepository = deviceRepository;
        _movementRepository = movementRepository;
    }

    public async Task MoveDeviceAsync(int deviceId, int newRoomId, int? newPosition = null, string? reason = null)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            throw new BusinessException($"Устройство с ID {deviceId} не найдено");

        if (device.CurrentRoomID == newRoomId)
            throw new BusinessException("Устройство уже находится в этом кабинете");

        var movement = new MovementHistory
        {
            DeviceID = deviceId,
            OldRoomID = device.CurrentRoomID,
            NewRoomID = newRoomId,
            Reason = reason,
            MoveDate = DateTime.UtcNow
        };

        await _movementRepository.AddMovementAsync(movement);

        device.CurrentRoomID = newRoomId;
        device.PositionInRoom = newPosition;   // если null, то без позиции
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task UpdateDeviceStatusAsync(int deviceId, int newStatusId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            throw new BusinessException($"Устройство с ID {deviceId} не найдено");

        device.StatusID = newStatusId;
        await _deviceRepository.UpdateAsync(device);
    }

    public Task<Device> GetDeviceByIdAsync(int deviceId)
        => _deviceRepository.GetByIdAsync(deviceId);

    public Task<IEnumerable<Device>> GetDevicesByRoomAsync(int roomId)
        => _deviceRepository.GetByRoomAsync(roomId);

    public Task<IEnumerable<Device>> GetDevicesWithOpenTicketsAsync()
        => _deviceRepository.GetDevicesWithOpenTicketsAsync();
}