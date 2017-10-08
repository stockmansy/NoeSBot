using NoeSbot.Enums;
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
            public NotifyEnum Type { get; set; }

            public string Key { get; set; }
        }

        public class NotifyVideo
        {
            public NotifyEnum Type { get; set; }

            public string Id { get; set; }
        }
    }
}
