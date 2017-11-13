using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NoeSbot.Resources;
using NoeSbot.Resources.Models;

namespace NoeSbot.Web.Controllers
{
    [Route("api/[controller]")]
    public class FeatureController : Controller
    {
        [HttpGet]
        public IEnumerable<ModuleInfoModel> GetFeatures()
        {
            var modules = Labels.GetModules();
            
            return modules;
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
