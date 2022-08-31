using Discord;
using Discord.Interactions;
using QualityEnsurance.Attributes;
using QualityEnsurance.Services;
using Microsoft.EntityFrameworkCore;

namespace QualityEnsurance.CommandModules
{
    public class WhitelistModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDbContextFactory<ApplicationContext> _contextFactory;

        public WhitelistModule(IDbContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        [SlashCommand("whitelist", "Get or set if the bot should only check whitelisted users.")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireUserPermissionOrOwner(GuildPermission.Administrator)]
        public async Task WhiteList(
            [Summary("id", "The id of the targeted activity.")]
            int id,
            [Summary("value", "What the whitelist status should be set to. Leave empty to read.")]
            bool? value = null,
            [Summary("user", "Specify a user to set or read the whitelisted status from.")]
            IUser user = null)
        {
            await DeferAsync(ephemeral: true);

            long guildId = (long)Context.Guild.Id;

            using var context = _contextFactory.CreateDbContext();
            var guild = context.GetGuild(guildId);
            var guildActivity = guild.GuildActivities.FirstOrDefault(ga => ga.IdWithinGuild == id);

            if (guildActivity == null)
            {
                await FollowupAsync($"No activity with id {id} could be found!", ephemeral: true);
                return;
            }


            if (value.HasValue)
            {
                if (user == null)
                {
                    if (guildActivity.RequireWhitelist == value)
                    {
                        if (value.Value)
                            await FollowupAsync($"Being whitelisted is already required for this activity.", ephemeral: true);
                        else 
                            await FollowupAsync($"Already ignoring whitelist for this activity.", ephemeral: true);
                    }
                    else
                    {
                        guildActivity.RequireWhitelist = value.Value;
                        if (value.Value)
                            await FollowupAsync($"Now only checking whitelisted users.", ephemeral: true);
                        else
                            await FollowupAsync($"Now checking all users.", ephemeral: true);
                    }
                }
                else
                {
                    var guildActivityUser = context.GetGuildActivityUser(guildActivity, context.GetUser((long)user.Id));
                    if (guildActivityUser.Whitelisted == value)
                    {
                        if (value.Value)
                            await FollowupAsync($"{user.Mention} is already whitelisted.", ephemeral: true);
                        else
                            await FollowupAsync($"{user.Mention} is already not whitelisted.", ephemeral: true);
                    }
                    else
                    {
                        guildActivityUser.Whitelisted = value.Value;
                        if (value.Value)
                            await FollowupAsync($"{user.Mention} is now whitelisted.", ephemeral: true);
                        else
                            await FollowupAsync($"{user.Mention} is no longer whitelisted.", ephemeral: true);
                    }
                }
                context.SaveChanges();
            } 
            else
            {
                if (user == null)
                {
                    if (guildActivity.RequireWhitelist)
                        await FollowupAsync($"Currently checking whitelisted users.", ephemeral: true);
                    else
                        await FollowupAsync($"Currently checking all users.", ephemeral: true);
                } 
                else
                {
                    var guildActivityUser = context.GetGuildActivityUser(guildActivity, context.GetUser((long)user.Id));
                    if (guildActivityUser.Whitelisted)
                        await FollowupAsync($"{user.Mention} is whitelisted.", ephemeral: true);
                    else
                        await FollowupAsync($"{user.Mention} is not whitelisted.", ephemeral: true);
                }
            }
        }
    }
}
