using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoeSbot.Converters;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoeSbot.Logic
{
    public class NotifyLogic
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationService _configurationService;
        private readonly INotifyService _notifyService;
        private readonly IHttpService _httpService;

        public NotifyLogic(DiscordSocketClient client, IConfigurationService configurationService, INotifyService notifyService, IHttpService httpService)
        {
            _client = client;
            _configurationService = configurationService;
            _httpService = httpService;
            _notifyService = notifyService;
        }

        public async Task Run(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var notifyItems = await _notifyService.RetrieveAllNotifysAsync();
                    foreach (var notifyItem in notifyItems)
                    {
                        var guild = _client.GetGuild((ulong)notifyItem.GuildId);
                        switch (notifyItem.Type)
                        {
                            case (int)NotifyEnum.Twitch:
                                try
                                {
                                    var content = await _httpService.SendTwitch(HttpMethod.Get, $"https://api.twitch.tv/kraken/streams/{notifyItem.Value}", "e0ggcjd1zomziofv6qrsa5gn1hf6d4");
                                    var response = await content.ReadAsStringAsync();
                                    var root = JsonConvert.DeserializeObject<TwitchStreamRoot>(response, new JsonSerializerSettings {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore,
                                        Formatting = Formatting.None,
                                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                        Converters = new List<JsonConverter> { new DecimalConverter() }
                                    });

                                    if (root.Stream != null)
                                    {
                                        var stream = root.Stream;
                                        
                                        var builder = new EmbedBuilder()
                                        {
                                            Color = Color.Red
                                        };

                                        var twitchName = (stream.Channel.DisplayName.Equals(stream.Channel.Name)) ? stream.Channel.DisplayName : $"{stream.Channel.DisplayName} ({stream.Channel.Name})";

                                        builder.AddField(x =>
                                        {
                                            x.Name = $"{twitchName}";
                                            x.Value = $"twitch user {twitchName} started streaming{Environment.NewLine}{Environment.NewLine}Game: {stream.Game}          Current viewers: {stream.ViewerCount}";
                                            x.IsInline = false;
                                        });

                                        if (stream.Channel.Logo != null)
                                            builder.WithThumbnailUrl(stream.Channel.Logo);

                                        await guild.DefaultChannel.SendMessageAsync("", false, builder.Build());
                                    }                                    
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(5000);
                                    continue;
                                }
                                break;
                        }
                    }

                    await Task.Delay(1000 * 60, cancelToken); //Wait 60 seconds between full updates

                    //var configs = await _configurationService.RetrieveAllOfTypeConfigurationsAsync((int)ConfigurationEnum.NotifySettings);
                    //foreach (var settings in _settings.AllServers)
                    //{
                    //    bool isServerUpdated = false;
                    //    foreach (var channelSettings in settings.Value.Channels)
                    //    {
                    //        bool isChannelUpdated = false;
                    //        var channel = _client.GetChannel(channelSettings.Key);
                    //        if (channel != null && channel.Server.CurrentUser.GetPermissions(channel).SendMessages)
                    //        {
                    //            foreach (var twitchStream in channelSettings.Value.Streams)
                    //            {
                    //                try
                    //                {
                    //                    var content = await _http.Send(HttpMethod.Get, $"https://api.twitch.tv/kraken/streams/{twitchStream.Key}");
                    //                    var response = await content.ReadAsStringAsync();
                    //                    JToken json = JsonConvert.DeserializeObject(response) as JToken;

                    //                    bool wasStreaming = twitchStream.Value.IsStreaming;
                    //                    string lastSeenGame = twitchStream.Value.CurrentGame;

                    //                    var streamJson = json["stream"];
                    //                    bool isStreaming = streamJson.HasValues;
                    //                    string currentGame = streamJson.HasValues ? streamJson.Value<string>("game") : null;

                    //                    if (wasStreaming) //Online
                    //                    {
                    //                        if (!isStreaming) //Now offline
                    //                        {
                    //                            _client.Log.Info("Twitch", $"{twitchStream.Key} is no longer streaming.");
                    //                            twitchStream.Value.IsStreaming = false;
                    //                            twitchStream.Value.CurrentGame = null;
                    //                            isChannelUpdated = true;
                    //                        }
                    //                        else if (lastSeenGame != currentGame) //Switched game
                    //                        {
                    //                            _client.Log.Info("Twitch", $"{twitchStream.Key} is now streaming {currentGame}.");
                    //                            twitchStream.Value.IsStreaming = true;
                    //                            twitchStream.Value.CurrentGame = currentGame;
                    //                            isChannelUpdated = true;
                    //                        }
                    //                    }
                    //                    else //Offline
                    //                    {
                    //                        if (isStreaming) //Now online
                    //                        {
                    //                            if (currentGame != null)
                    //                                _client.Log.Info("Twitch", $"{twitchStream.Key} has started streaming {currentGame}.");
                    //                            else
                    //                                _client.Log.Info("Twitch", $"{twitchStream.Key} has started streaming.");
                    //                            await channel.SendMessage(Format.Escape($"{twitchStream.Key} is now live (http://www.twitch.tv/{twitchStream.Key})."));
                    //                            twitchStream.Value.IsStreaming = true;
                    //                            twitchStream.Value.CurrentGame = currentGame;
                    //                            isChannelUpdated = true;
                    //                        }
                    //                    }
                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    _client.Log.Error("Twitch", ex);
                    //                    await Task.Delay(5000);
                    //                    continue;
                    //                }
                    //            }
                    //        } //Stream Loop

                    /*if (channelSettings.Value.UseSticky && (isChannelUpdated || channelSettings.Value.StickyMessageId == null))
                    {
                        //Build the sticky post
                        builder.Clear();
                        builder.AppendLine(Format.Bold("Current Streams:"));
                        foreach (var stream in channelSettings.Value.Streams)
                        {
                            var streamData = stream.Value;
                            if (streamData.IsStreaming)
                            {
                                if (streamData.CurrentGame != null)
                                    builder.AppendLine(Format.Escape($"{stream.Key} - {streamData.CurrentGame} (http://www.twitch.tv/{stream.Key})"));
                                else
                                    builder.AppendLine(Format.Escape($"{stream.Key} (http://www.twitch.tv/{stream.Key}))"));
                            }
                        }
                        //Edit the old message or make a new one
                        string text = builder.ToString();
                        if (channelSettings.Value.StickyMessageId != null)
                        {
                            try
                            {
                                await _client.StatusAPI.Send(
                                    new UpdateMessageRequest(channelSettings.Key, channelSettings.Value.StickyMessageId.Value) { Content = text });
                            }
                            catch (HttpException)
                            {
                                _client.Log.Error("Twitch", "Failed to edit message.");
                                channelSettings.Value.StickyMessageId = null;
                            }
                        }
                        if (channelSettings.Value.StickyMessageId == null)
                        {
                            channelSettings.Value.StickyMessageId = (await _client.SendMessage(_client.GetChannel(channelSettings.Key), text)).Id;
                            isChannelUpdated = true;
                        }
                        //Delete all old messages in the sticky'd channel to keep our message at the top
                        try
                        {
                            var msgs = await _client.DownloadMessages(channel, 50);
                            foreach (var message in msgs
                                    .OrderByDescending(x => x.Timestamp)
                                    .Where(x => x.Id != channelSettings.Value.StickyMessageId)
                                    .Skip(3))
                                await _client.DeleteMessage(message);
                        }
                        catch (HttpException) { }
                    }*/
                    //isServerUpdated |= isChannelUpdated;
                } //Channel Loop
                          //if (isServerUpdated)
                          // await _settings.Save(settings);
                        //} //Server Loop
                //}

               
            }
            catch (TaskCanceledException) { }
        }
    }
}
