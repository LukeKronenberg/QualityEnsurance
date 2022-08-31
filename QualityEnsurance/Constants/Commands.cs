using Discord;
using Discord.Commands;

namespace QualityEnsurance.Constants
{
    public static class Commands
    {
        private static List<Command> _commandsCache = null;
        public static List<Command> GetCommands()
        {
            if (_commandsCache != null)
                return _commandsCache;
            // Gets only static classes
            var commands = typeof(Commands).GetNestedTypes().Where(t => t.IsAbstract && t.IsSealed);
            _commandsCache = commands.Select(c => new Command(c)).ToList();
            return _commandsCache;
        }

        // Use custom AliasAttribute to allow for arrays to be used as parameter
        public static class Help
        {
            public const string Name = "help";
            public const string DescriptionBasic = "Shows this menu";
            public const string DescriptionFull = "Placeholder";
            public static readonly Syntax[] Syntaxes = 
            { 
                new() { Description = "Show all commands" },
                new() { 
                    Description = "Show info for specific command", 
                    Parameters = new Parameter[] { 
                        new() { Name = "Command Name", Type = "text" },
                    }
                },
            };
        }

        public static class Whitelist
        {
            public const string Name = "whitelist";
            public const string DescriptionBasic = "Set/Get whitelisted for guild or specific users.";
            public const string DescriptionFull = "Get or set if the current guild should only check whitlisted users. Also get or set the whitelisted status on individual users.";
            public static readonly Syntax[] Syntaxes =
            {
                new() { Description = "Returns the current state of the whitelist for current guild." },
                new() {
                    Description = "Enable or disable the whitelist for the current guild.",
                    Parameters = new Parameter[] {
                        new() { Name = "Value", Type = "true|false" },
                    }
                },
                new() {
                    Description = "Returns if user is whitelisted in the current guild.",
                    Parameters = new Parameter[] {
                        new() { Name = "User", Type = "@user" },
                    }
                },
                new() {
                    Description = "Whitelist a user for in the current guild.",
                    Parameters = new Parameter[] {
                        new() { Name = "User", Type = "@user" },
                        new() { Name = "Value", Type = "true|false" },
                    }
                },
            };
        }

        public static class GuildActivities
        {
            public const string Name = "activity list";
            public const string DescriptionBasic = "Lists all registered activities for this guild.";
            public const string DescriptionFull = DescriptionBasic;
            public static readonly Syntax[] Syntaxes =
            {
                new() { Description = DescriptionBasic }
            };
        }

        public static class AddActivity
        {
            public const string Name = "activity add";
            public const string DescriptionBasic = "Register a new activity.";
            public const string DescriptionFull = "Register a activity which the bot will listen to. The bot will match the activities name, Application-Id or Spotify-Id.";
            public static readonly Syntax[] Syntaxes =
            {
                new() 
                { 
                    Parameters = new Parameter[] {
                        new() { Name = "name", Type = "text", Description = "The name of the activity. Checking for this value is not safe and can result in errors. Cannot be used with \"spotify-id\"."},
                        new() { Name = "app-id", Type = "number", Description = "The id of an application. Use \"/activity list @user\" to get ids if available. Cannot be used with \"spotify-id\"."},
                        new() { Name = "spotify-id", Type = "text", Description = "The id of a song on spotify. Use \"/activity list @user\" while they are listening to it. Cannot be used with \"name\" and \"app-id\"."},
                        new() { Name = "action", Type = "select", Description = "Choose what should be done when an user has this activity active for \"countdown-duration\"."},
                        new() { Name = "countdown-duration", Type = "number", Description = "Time in seconds after which the selected action should be executed."},
                        new() { Name = "timeout-duration", Type = "number", Description = "Time in seconds for which an user gets timedout. Only used when \"action\" is timeout."},
                        new() { Name = "start-message", Type = "text", Description = "The message that gets sent in dms when a user starts an registered activity."},
                        new() { Name = "action-message", Type = "text", Description = "The message which gets sent when the action gets executed."},
                    }
                }
            };
        }
        public static class RemoveActivity
        {
            public const string Name = "activity remove";
            public const string DescriptionBasic = "Remove a registered activity";
            public const string DescriptionFull = "Remove a registered activity by its id. This deletion is permanent.";
            public static readonly Syntax[] Syntaxes =
            {
                new()
                {
                    Description = "Remove a registered activity for the current guild.",
                    Parameters = new Parameter[] {
                        new() { Name = "id", Type = "number", Description = "The id of the targeted activity. The id can be seen with the \"/activity list\" command." }
                    }
                }
            };
        }
        public static class EditActivity
        {
            public const string Name = "activity edit";
            public const string DescriptionBasic = "Edit a registered activity.";
            public const string DescriptionFull = "Placeholder";
            public static readonly Syntax[] Syntaxes =
            {
                new()
                {
                    Description = "Change an registered activity for the current guild.",
                    Parameters = new Parameter[] {
                        new() { Name = "id", Type = "number", Description = "The id of the targeted activity. The id can be seen with the \"/activity list\" command." },
                        new() { Name = "action", Type = "select", Description = "Choose what should be done when an user has this activity active for \"countdown-duration\"."},
                        new() { Name = "countdown-duration", Type = "number", Description = "Time in seconds after which the selected action should be executed."},
                        new() { Name = "timeout-duration", Type = "number", Description = "Time in seconds for which an user gets timedout. Only used when \"action\" is timeout."},
                        new() { Name = "start-message", Type = "text", Description = "The message that gets sent in dms when a user starts an registered activity."},
                        new() { Name = "action-message", Type = "text", Description = "The message which gets sent when the action gets executed."},
                    }
                }
            };
        }
        public static class RemoveAllActivities
        {
            public const string Name = "activity remove-all";
            public const string DescriptionBasic = "Removes all registerd activities permanetly.";
            public const string DescriptionFull = DescriptionBasic;
            public static readonly Syntax[] Syntaxes =
            {
                new() { Description = DescriptionBasic }
            };
        }

        public static class ListPendingActions
        {
            public const string Name = "list";
            public const string DescriptionBasic = "List all pending actions and their ETA";
            public const string DescriptionFull = "Placeholder";
            public static readonly Syntax[] Syntaxes =
            {
                new() { Description = "List all pending actions for users in this guild and there ETA." }
            };
        }
    }
}
