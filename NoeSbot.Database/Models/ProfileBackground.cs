using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class ProfileBackground
    {
        [Key]
        public int ProfileBackgroundId { get; set; }
        public long GuildId { get; set; }
        public long? UserId { get; set; }
        public IList<ProfileBackgroundAlias> Aliases { get; set; }
        public ProfileBackgroundSetting ProfileBackgroundSettingId { get; set; }
        public string Value { get; set; }

        public enum ProfileBackgroundSetting {
            Game = 1,
            Custom = 2
        }

        public class ProfileBackgroundAlias
        {
            [Key]
            public int AliasId { get; set; }
            public string Alias { get; set; }
        }
    }
}
