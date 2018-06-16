using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using NoeSbot.Database.Models;
using NoeSbot.Database.ViewModels;

namespace NoeSbot.Database.Services
{
    public interface IActivityLogService
    {
        Task<bool> AddActivityLog(long guildId, long userId, long channelId, string command, string log);
        Task<ActivityLogVM> RetrieveActivityLog(long guildId, long? userId = null);
    }
}
