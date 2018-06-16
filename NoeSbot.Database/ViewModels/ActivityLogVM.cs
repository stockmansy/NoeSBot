using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Database.ViewModels
{
    public class ActivityLogVM
    {
        public ActivityLogVM(int id, long guildId, IEnumerable<ActivityLogVMItem> logs = null)
        {
            Id = id;
            GuildId = guildId;
            Logs = logs;

            if (logs == null)
                Logs = new List<ActivityLogVMItem>();
        }

        public int Id { get; private set; }
        public long GuildId { get; private set; }
        public IEnumerable<ActivityLogVMItem> Logs { get; private set; }

        public class ActivityLogVMItem
        {
            public ActivityLogVMItem(int id, long userId, long channelId, string command, string log, DateTime date)
            {
                Id = id;
                UserId = userId;
                ChannelId = channelId;
                Command = command;
                Log = log;
                Date = date;
            }

            public int Id { get; private set; }
            public long UserId { get; private set; }
            public long ChannelId { get; private set; }
            public string Command { get; private set; }
            public string Log { get; private set; }
            public DateTime Date { get; private set; }
        }
    }
}
