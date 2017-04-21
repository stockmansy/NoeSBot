using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleNameAttribute : Attribute
    {
        private ModuleEnum Name;

        public ModuleNameAttribute(ModuleEnum name)
        {
            Name = name;
        }
    }
}
