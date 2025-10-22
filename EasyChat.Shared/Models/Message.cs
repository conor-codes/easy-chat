using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyChat.Shared.Models
{
    public class Message
    {
        public string Id {  get; set; } = Guid.NewGuid().ToString();
        public string RoomId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
