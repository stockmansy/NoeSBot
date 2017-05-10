using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class MessageTrigger
    {
        [Key]
        public int MessageTriggerId { get; set; }
        public string Trigger { get; set; }
        public string Message { get; set; }
        public long Server { get; set; }
    }
}
