using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Database.Models
{
    public abstract class BaseModel
    {
        public DateTime CreationDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
