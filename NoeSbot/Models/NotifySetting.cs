using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class NotifySetting
    {
        public IList<NotifyStream> Streams { get; set; }
        public IList<NotifyVideo> Videos { get; set; }

        public class NotifyStream
        {
            public NotifyServiceEnum Type { get; set; }

            public string Key { get; set; }
        }

        public class NotifyVideo
        {
            public NotifyServiceEnum Type { get; set; }

            public string Id { get; set; }
        }
    }

    public enum NotifyServiceEnum
    {
        Twitch = 1,
        Youtube = 2
    }
}
