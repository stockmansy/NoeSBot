using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database
{
    public class Config
    {
        [Key]
        public int ConfigurationId { get; set; }
        public long GuildId { get; set; }
        public int ConfigurationTypeId { get; set; }
        public string Value { get; set; }
    }
}
