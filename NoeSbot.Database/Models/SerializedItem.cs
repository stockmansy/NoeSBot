using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class SerializedItem : BaseModel
    {
        [Key]
        public int SerializedItemId { get; set; }

        public long GuildId { get; set; }

        public int Type { get; set; }

        [MaxLength]
        public string Content { get; set; }

        public enum SerializedItemType
        {
            GuildBackup = 1
        }
    }
}
