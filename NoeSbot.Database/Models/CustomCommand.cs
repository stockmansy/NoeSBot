using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class CustomCommand
    {
        [Key]
        public int CustomCommandId { get; set; }
        public CustomCommandType Type { get; set; }
        public long GuildId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public enum CustomCommandType
        {
            Punish,
            Unpunish,
            Alias
        }
    }
}
