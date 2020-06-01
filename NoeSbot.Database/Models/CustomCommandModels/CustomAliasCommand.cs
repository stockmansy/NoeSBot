using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class CustomAliasCommand
    {
        public string CommandName { get; set; }
        public string AliasCommand { get; set; }
        public bool RemoveResult { get; set; }
    }
}
