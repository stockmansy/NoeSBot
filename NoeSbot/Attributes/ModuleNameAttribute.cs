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
    public class ModuleNameAttribute : PreconditionAttribute
    {
        private ModuleEnum _module;

        public ModuleNameAttribute(ModuleEnum module)
        {
            _module = module;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var loadedModules = Configuration.Load(context.Guild.Id).LoadedModules;
            if (loadedModules.Contains((int)_module))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Module not loaded."));
        }
    }
}
