using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class CustomPunishItem
    {
        public bool HasCustom { get; set; }
        public string DelayMessage { get; set; }
        public string ReasonMessage { get; set; }
        public string PunishTime { get; set; }
    }
}
