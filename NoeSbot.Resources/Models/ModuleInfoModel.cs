using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Resources.Models
{
    public class ModuleInfoModel
    {
        public string Name { get; set; }
        public ModuleEnum Id { get; set; }
        public IList<CommandInfoModel> Commands { get; set; }
    }
}
