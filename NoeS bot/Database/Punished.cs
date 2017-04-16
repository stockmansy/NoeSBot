using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database
{
    public class Punished
    {
        public int PunishedId { get; set; }
        public long UserId { get; set; }
        public DateTime TimeOfPunishment { get; set; }
        public int Duration { get; set; }
        public string Reason { get; set; }
    }
}
