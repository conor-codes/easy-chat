using EasyChat.Api.Data;
using EasyChat.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasyChat.Api.Hubs;

[Authorize(AuthenticationSchemes = "Bearer")]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _db;
    public ChatHub(ApplicationDbContext db) 
    {
        _db = db; 
    }

    private string UserId => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? Context.UserIdentifier
                        ?? Context.ConnectionId;

    private static string GroupName(Guid roomId) => $"room:{roomId}";

    public async Task JoinRoom(Guid roomId)
    {
        // Ensure room exists 
        var exists = await _db.Rooms.AsNoTracking().AnyAsync(r => r.Id == roomId.ToString());
        if (!exists) 
            Console.WriteLine("Room not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(roomId));
        await Clients.Caller.SendAsync("JoinedRoom", roomId);
    }


    public async Task SendMessage(Guid roomId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        // persist
        var msg = new Message
        {
            Id = Guid.NewGuid().ToString(),
            RoomId = roomId.ToString(),
            SenderId = UserId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        // broadcast saved record
        await Clients.Group(GroupName(roomId)).SendAsync("MessageReceived", new
        {
            msg.Id,
            msg.RoomId,
            msg.SenderId,
            msg.Content,
            msg.SentAt
        });
    }

    public Task LeaveRoom(Guid roomId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(roomId));
    }
}