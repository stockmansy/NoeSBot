using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NoeSbot.Models
{
    public class TwitchUsersRoot
    {
        [JsonProperty("_total")]
        public int Total { get; set; }

        [JsonProperty("users")]
        public TwitchUser[] Users { get; set; }
    }

    public class TwitchUser
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }        

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("created_at")]
        public DateTime Created_at { get; set; }

        [JsonProperty("updated_at")]
        public DateTime Updated_at { get; set; }
    }
}
