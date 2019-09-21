using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NoeSbot.Database.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Discord.Net;
using System.Net;

namespace NoeSbot.Database.Services
{
    public class HttpService : IHttpService
    {
        private HttpClient _http;
        private readonly ILogger<ConfigurationService> _logger;

        public HttpService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConfigurationService>();
            _http = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseCookies = false,
                PreAuthenticate = false //We do auth ourselves
            });
            _http.DefaultRequestHeaders.Add("accept", "*/*");
            _http.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
        }

        #region Config

        public Task<HttpContent> SendTwitch(HttpMethod method, string path, string authToken = null) => SendTwitch<object>(method, path, null, authToken);

        private async Task<HttpContent> SendTwitch<T>(HttpMethod method, string path, T payload, string authToken = null)
            where T : class
        {
            var msg = new HttpRequestMessage(method, path);

            if (authToken != null)
            {
                msg.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                msg.Headers.Add("Client-ID", authToken);
            }

            if (payload != null)
            {
                string json = JsonConvert.SerializeObject(payload);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            msg.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");


            var response = await _http.SendAsync(msg, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.ToString());
            return response.Content;
        }

        public Task<HttpContent> Send(HttpMethod method, string path) => Send<object>(method, path, null);

        private async Task<HttpContent> Send<T>(HttpMethod method, string path, T payload)
            where T : class
        {
            var msg = new HttpRequestMessage(method, path);
            
            if (payload != null)
            {
                string json = JsonConvert.SerializeObject(payload);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            msg.Headers.Add("Accept", "application/json");

            var response = await _http.SendAsync(msg, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.ToString());
            return response.Content;
        }

        #endregion
    }
}
