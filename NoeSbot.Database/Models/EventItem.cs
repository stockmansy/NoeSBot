using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NoeSbot.Database.Models
{
    public class EventItem : BaseModel
    {
        [Key]
        public int EventItemId { get; set; }
        public long GuildId { get; set; }
        public string UniqueIdentifier { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public DateTime Date { get; set; }
        public DateTime? MatchDate { get; set; }
        public bool Active { get; set; }
        public IList<Participant> Participants { get; set; }
        public IList<Organiser> Organisers { get; set; }

        public class Participant
        {
            [Key]
            public int ParticipantId { get; set; }
            public long UserId { get; set; }
            public long? MatchUserId { get; set; }
        }

        public class Organiser
        {
            [Key]
            public int OrganiserId { get; set; }
            public long UserId { get; set; }
        }
    }
}
