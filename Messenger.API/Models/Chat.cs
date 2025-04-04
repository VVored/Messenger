﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Messenger.API.Models
{
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }

        [Required]
        [MaxLength(10)]
        public string ChatType { get; set; } // 'private' или 'group'

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
        public GroupChatInfo GroupChatInfo { get; set; }
    }
}
