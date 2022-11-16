using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using QualityEnsurance.Extensions;

namespace QualityEnsurance.Attributes
{
    public class RequireOwnerOrConfigWhitelist : RequireOwnerAttribute
    {
        public RequireOwnerOrConfigWhitelist() : base()
        {

        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            IConfiguration config = (IConfiguration)services.GetService(typeof(IConfiguration));

            var whitelistedOwners = config.GetBotOwners();

            if (Array.IndexOf(whitelistedOwners, context.User.Id) != -1)
                return PreconditionResult.FromSuccess();

            return await base.CheckRequirementsAsync(context, command, services);
        }
    }
}
