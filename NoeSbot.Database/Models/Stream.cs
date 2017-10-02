using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class Stream
    {
        [Key]
        public int StreamId { get; set; }
        public long GuildId { get; set; }
        public long UserId { get; set; }
        public ICollection<ProfileItem> Items { get; set; }
    }
}
