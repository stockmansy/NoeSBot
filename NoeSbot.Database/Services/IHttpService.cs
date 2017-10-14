using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using NoeSbot.Database.Models;
using System.Net.Http;

namespace NoeSbot.Database.Services
{
    public interface IHttpService
    {
        Task<HttpContent> SendTwitch(HttpMethod method, string path, string authToken = null);
        Task<HttpContent> Send(HttpMethod method, string path);
    }
}
