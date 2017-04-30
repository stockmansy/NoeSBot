using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class PollModel
    {
        public PollModel()
        {
            Question = "";
            Options = new Dictionary<int, string>();
            UsersWhoVoted = new Dictionary<IUser, int>();
            Votes = new Dictionary<int, int>();
            ShowWhoVoted = false;
            Duration = 60;
        }

        public string Question { get; set; }
        public Dictionary<int, string> Options { get; set; }
        public Dictionary<IUser, int> UsersWhoVoted { get; set; }
        public Dictionary<int, int> Votes { get; set; }
        public int Duration { get; set; }
        public bool ShowWhoVoted { get; set; }
    }
}
