using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NoeSbot.Models
{
    public class YoutubeStream
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("pageInfo")]
        public YoutubePageInfo PageInfo { get; set; }

        [JsonProperty("items")]
        public YoutubeVideoItem[] Items { get; set; }

        public class YoutubePageInfo
        {
            [JsonProperty("totalResults")]
            public int TotalResults { get; set; }

            [JsonProperty("resultsPerPage")]
            public int ResultsPerPage { get; set; }
        }

        public class YoutubeVideoItem
        {
            [JsonProperty("kind")]
            public string Kind { get; set; }

            [JsonProperty("etag")]
            public string Etag { get; set; }

            [JsonProperty("id")]
            public YoutubeVideoItemId Id { get; set; }

            [JsonProperty("snippet")]
            public YoutubeVideoItemSnippet Snippet { get; set; }

            public class YoutubeVideoItemId
            {
                [JsonProperty("kind")]
                public string Kind { get; set; }

                [JsonProperty("videoId")]
                public string VideoId { get; set; }
            }

            public class YoutubeVideoItemSnippet
            {
                [JsonProperty("publishedAt")]
                public DateTime PublishedAt { get; set; }

                [JsonProperty("ChannelId")]
                public string ChannelId { get; set; }

                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("description")]
                public string Description { get; set; }

                [JsonProperty("thumbnails")]
                public Thumbnail Thumbnails { get; set; }

                [JsonProperty("channelTitle")]
                public string ChannelTitle { get; set; }

                [JsonProperty("liveBroadcastContent")]
                public string LiveBroadcastContent { get; set; }

                public class Thumbnail
                {
                    [JsonProperty("default")]
                    public ThumbnailItem Default { get; set; }

                    [JsonProperty("medium")]
                    public ThumbnailItem Medium { get; set; }

                    [JsonProperty("high")]
                    public ThumbnailItem High { get; set; }

                    public class ThumbnailItem
                    {
                        [JsonProperty("url")]
                        public string Url { get; set; }
                    }
                }
            }
        }
    }
}