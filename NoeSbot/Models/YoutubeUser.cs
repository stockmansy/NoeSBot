using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NoeSbot.Models
{
    public class YoutubeUserRoot
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("pageInfo")]
        public YoutubePageInfo PageInfo { get; set; }

        [JsonProperty("items")]
        public YoutubeUser[] Items { get; set; }

        public class YoutubePageInfo
        {
            [JsonProperty("totalResults")]
            public int TotalResults { get; set; }

            [JsonProperty("resultsPerPage")]
            public int ResultsPerPage { get; set; }
        }

        public class YoutubeUser
        {
            [JsonProperty("kind")]
            public string Kind { get; set; }

            [JsonProperty("etag")]
            public string Etag { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }
        }
    }
}
