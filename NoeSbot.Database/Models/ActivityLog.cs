using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class ActivityLog : BaseModel
    {
        public ActivityLog()
        {
            Logs = new List<ActivityLogItem>();
        }

        [Key]
        public int ActivityLogId { get; set; }
        public long GuildId { get; set; }
        public ICollection<ActivityLogItem> Logs { get; set; }

        public class ActivityLogItem : BaseModel
        {
            [Key]
            public int ActivityLogItemId { get; set; }
            public long UserId { get; set; }
            public long ChannelId { get; set; }
            public string Command { get; set; }
            public string Log { get; set; }
        }
    }
}
