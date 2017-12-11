using NoeSbot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class EventItem
    {
        public int Id { get; set; }
        public EventEmum Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UniqueIdentifier { get; set; }
        public DateTime Date { get; set; }
        public DateTime? MatchDate { get; set; }
    }
}
