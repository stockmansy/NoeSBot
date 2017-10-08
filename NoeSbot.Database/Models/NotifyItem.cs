using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class NotifyItem
    {
        [Key]
        public int NotifyItemId { get; set; }
        public long GuildId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Logo { get; set; }
        public IList<NotifyUser> Users { get; set; }
        public IList<NotifyRole> Roles { get; set; }
        public int Type { get; set; }

        public class NotifyUser
        {
            [Key]
            public int NotifyUserId { get; set; }
            public long UserId { get; set; }
        }

        public class NotifyRole
        {
            [Key]
            public int NotifyRoleId { get; set; }
            public string Rolename { get; set; }
            public long RoleId { get; set; }
        }
    }
}
