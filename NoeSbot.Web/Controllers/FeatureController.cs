using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NoeSbot.Resources;
using NoeSbot.Resources.Models;

namespace NoeSbot.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureController : ControllerBase
    {
        [HttpGet]
        [Route("all")]
        public IEnumerable<ModuleInfoModel> GetFeatures()
        {
            var modules = Labels.GetModules();
            
            foreach (var module in modules)
            {
                foreach (var command in module.Commands)
                {
                    command.Examples = command.Examples.Select(x => string.Format(x, '~', command.Command.ToLower())).ToList();
                }
            }

            return modules;
        }
    }
}
