using Discord;
using Discord.Interactions;

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
