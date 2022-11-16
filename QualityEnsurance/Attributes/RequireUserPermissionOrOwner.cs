using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using QualityEnsurance.Extensions;

namespace QualityEnsurance.Attributes
{
    public class RequireUserPermissionOrOwner : RequireUserPermissionAttribute
    {
        public RequireUserPermissionOrOwner(GuildPermission guildPermission) : base(guildPermission)
        {
        }
        public RequireUserPermissionOrOwner(ChannelPermission channelPermission) : base(channelPermission)
        {
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            IConfiguration config = (IConfiguration)services.GetService(typeof(IConfiguration));

            long[] whitelistedOwners = config.GetArray<long>("whitelistedOwners");

            if (Array.IndexOf(whitelistedOwners, context.User.Id) != -1)
                return PreconditionResult.FromSuccess();

            if (context.Client.TokenType == TokenType.Bot)
            {
                IApplication application = await context.Client.GetApplicationInfoAsync();
                if (application.Owner.Id == context.User.Id)
                    return PreconditionResult.FromSuccess();
            }

            return await base.CheckRequirementsAsync(context, commandInfo, services);
        }
    }
}
