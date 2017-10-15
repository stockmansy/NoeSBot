using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using NoeSbot.Database.Models;

namespace NoeSbot.Database.Services
{
    public interface INotifyService
    {
        Task<bool> AddNotifyItem(long guildId, long userId, string name, string value, string logo, int type);
        Task<bool> AddUserToNotifyItem(long userId, int notifyItemId);
        Task<bool> AddNotifyItemRole(long guildId, long roleId, string name, string value, string logo, int type);
        Task<bool> RemoveNotifyItem(long guildId, string name, int type);
        Task<bool> RemoveUserFromNotifyItem(long userId, int notifyItemId);
        Task<List<NotifyItem>> RetrieveAllNotifysAsync();
        Task<NotifyItem> RetrieveNotifyAsync(long guildId, string name, int type);
    }
}
