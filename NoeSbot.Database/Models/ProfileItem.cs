using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class ProfileItem
    {
        [Key]
        public int ProfileItemId { get; set; }
        public int ProfileId { get; set; }
        public Profile Profile { get; set; }
        public int ProfileItemTypeId { get; set; }
        public string Value { get; set; }
    }
}
