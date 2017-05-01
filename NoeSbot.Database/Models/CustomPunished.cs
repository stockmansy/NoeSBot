using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database
{
    public class CustomPunished
    {
        [Key]
        public int CustomPunishedId { get; set; }
        public long UserId { get; set; }
        public string Reason { get; set; }
        public string DelayMessage { get; set; }
    }
}
