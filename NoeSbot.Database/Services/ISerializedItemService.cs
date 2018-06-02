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
    public interface ISerializedItemService
    {
        Task<bool> AddGuildBackup(long guildId, object backup);
    }
}
