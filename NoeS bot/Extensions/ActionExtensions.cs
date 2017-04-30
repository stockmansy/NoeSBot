using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Extensions
{
    public static class ActionExtensions
    {
        public static async void DelayFor(this Action action, TimeSpan delay)
        {
            await Task.Delay(delay);
            action();
        }
    }
}
