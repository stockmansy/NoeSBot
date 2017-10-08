using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NoeSbot.Models
{
    public class TwitchStreamRoot
    {
        [JsonProperty("stream")]
        public TwitchStream Stream { get; set; }
    }

    public class TwitchStream
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("viewers")]
        public int ViewerCount { get; set; }

        [JsonProperty("video_height")]
        public int VideoHeight { get; set; }

        [JsonProperty("average_fps")]
        public decimal AverageFps { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("is_playlist")]
        public bool IsPlaylist { get; set; }

        [JsonProperty("preview")]
        public TwitchStreamPreview Preview { get; set; }

        [JsonProperty("channel")]
        public TwitchStreamChannel Channel { get; set; }
    }

    public class TwitchStreamPreview
    {
        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }
    }

    public class TwitchStreamChannel
    {
        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("broadcaster_language")]
        public string BroadcasterLanguage { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("_id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("partner")]
        public bool Partner { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("video_banner")]
        public string VideoBanner { get; set; }

        [JsonProperty("profile_banner")]
        public string ProfileBanner { get; set; }

        [JsonProperty("profile_banner_background_color")]
        public string ProfileBannerBackgroundColor { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("followers")]
        public int Followers { get; set; }
    }
}
