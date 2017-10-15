using NoeSbot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class NotifyItem
    {
        public int Id { get; set; }
        public NotifyEnum Type { get; set; }
    }
}
