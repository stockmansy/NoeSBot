using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class CustomCommand
    {
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public CustomPunishCommand PunishCommand { get; set; }
        public CustomUnPunishCommand UnPunishCommand { get; set; }

        public class CustomPunishCommand
        {
            public ulong UserId { get; set; }
            public int DurationInSec { get; set; }
            public string Reason { get; set; }
        }

        public class CustomUnPunishCommand
        {
            public ulong UserId { get; set; }
        }
    }
}
