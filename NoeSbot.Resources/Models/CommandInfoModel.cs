using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Resources.Models
{
    public class CommandInfoModel
    {
        public string Name { get; set; }
        public IList<string> Alias { get; set; }
        public string Description { get; set; }
        public IList<FieldInfoModel> Fields { get; set; }
        public IList<string> Examples { get; set; }
    }
}
