using System;
using AppIt.Data;

namespace AppIt.Data.Entities
{
    public class Notification 
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
