using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Enums;
using NoeSbot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BotAccessAttribute : PreconditionAttribute
    {
        private AccessLevel _accessLevel;

        public BotAccessAttribute(AccessLevel accessLevel)
        {
            _accessLevel = accessLevel;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            switch (_accessLevel)
            {
                case AccessLevel.BotsRefused:
                    if (!context.Message.Author.IsBot && !context.Message.Author.IsWebhook)
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    return Task.FromResult(PreconditionResult.FromError("Bots are not allowed to run this command"));
                case AccessLevel.BotSelfAllowed:
                    if (context.Message.Author.IsWebhook)
                        return Task.FromResult(PreconditionResult.FromError("Webhook is not allowed to run this command"));

                    if (context.Message.Author.IsBot) {
                       if (context.Message.Author.Id == context.Client.CurrentUser.Id)
                            return Task.FromResult(PreconditionResult.FromSuccess());
                        return Task.FromResult(PreconditionResult.FromError("Other bots are not allowed to run this command"));
                    }

                    return Task.FromResult(PreconditionResult.FromSuccess());
                case AccessLevel.BotsAllowed:
                default:
                    return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }

        public enum AccessLevel
        {
            BotsAllowed,
            BotsRefused,
            BotSelfAllowed
        }
    }
}
