using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class CustomPunishCommand
    {
        public long UserId { get; set; }
        public int Duration { get; set; }
        public string Reason { get; set; }
    }
}
