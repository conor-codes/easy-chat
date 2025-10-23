using EasyChat.Api.Data;
using EasyChat.Shared.Models.EasyChat.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyChat.Api.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public RoomsController(ApplicationDbContext db) => _db = db;

        [HttpGet("{roomId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid roomId, [FromQuery] int take = 50, [FromQuery] DateTime? before = null)
        {
            take = Math.Clamp(take, 1, 200);

            var q = _db.Messages.AsNoTracking()
                .Where(m => m.RoomId == roomId.ToString());

            if (before.HasValue)
                q = q.Where(m => m.SentAt < before.Value);

            var items = await q
                .OrderByDescending(m => m.SentAt)
                .Take(take)
                .Select(m => new {
                    m.Id,
                    m.RoomId,
                    m.SenderId,
                    m.Content,
                    m.SentAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // Create a room
        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] string name)
        {
            var room = new Room { Id = Guid.NewGuid().ToString(), Name = name, CreatedAt = DateTime.UtcNow };
            _db.Add(room);
            await _db.SaveChangesAsync();
            return Ok(new { room.Id, room.Name });
        }
    }
}
