using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SharpExchange.Chat.Actions;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;
using StackoverflowChatbot.Relay;

namespace StackoverflowChatbot
{
	internal static class Discord
	{
		private static DiscordSocketClient? _client;

		internal static async Task<DiscordSocketClient> GetDiscord() => _client ??= await CreateDiscordClient();

		internal static Dictionary<int, RoomWatcher<DefaultWebSocket>> StackRoomWatchers = new Dictionary<int, RoomWatcher<DefaultWebSocket>>();
		internal static Dictionary<int, ActionScheduler> StackSchedulers = new Dictionary<int, ActionScheduler>();

		private static async Task<DiscordSocketClient> CreateDiscordClient()
		{
			var config = new DiscordSocketConfig();
			config.AlwaysDownloadUsers = true;
			
			var client = new DiscordSocketClient(config);
			
			//Setuo handlers
			client.MessageReceived += ClientRecieved;
			//Logs in
			await client.LoginAsync(TokenType.Bot, Config.Manager.Config().DiscordToken);
			await client.StartAsync();
			//Now wr're done
			return client;
		}

		private static async Task ClientRecieved(SocketMessage arg)
		{

			if (arg.Author is SocketGuildUser user)
			{
				if (arg.Author.IsBot) return;

				var config = Config.Manager.Config();
				Console.WriteLine($"[DIS {arg.Channel.Name}] {arg.Content}");
				//Check if we have a mapping.
				if (config.DiscordToStackMap.ContainsKey(arg.Channel.Name))
				{
					//We are setup to map this channel's messages to stack.
					var roomId = config.DiscordToStackMap[arg.Channel.Name];
					//Build the message
					//var displayname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;

					// Create a list of messages in case there are embedded codeblocks or stuff alongside text
					var messages = FromDiscordExtensions.BuildSoMessage(user, config, arg);

					foreach (var message in messages)
					{
						//Find the room scheduler
						if (StackSchedulers.ContainsKey(roomId))
						{
							//We already have a scheduler, lets goooo
							var sched = StackSchedulers[roomId];
							await sched.CreateMessageAsync(message);
						}
						//Or create one if we already have a watcher.
						else if (StackRoomWatchers.ContainsKey(roomId))
						{
							var watcher = StackRoomWatchers[roomId];
							var newScheduler = new ActionScheduler(watcher.Auth, RoomService.Host, roomId);
							StackSchedulers.Add(roomId, newScheduler);
							await newScheduler.CreateMessageAsync(message);

							await arg.Channel.SendMessageAsync("Opened a new scheduler for sending messages to Stack. FYI.");
						}
						else
						{
							//or complain about not watching stack.
							await arg.Channel.SendMessageAsync(
								"Unable to sync messages to Stack - I'm not watching the corresponding channel. Invite me to the channel on stack and tryagain.");
						}
					}
				}
			}
		}
	
		internal static IEnumerable<SocketGuildUser> GetUserByName(string name, bool onlyExactMatch = false)
		{
			var discordClient = Discord.GetDiscord().GetAwaiter().GetResult();
			var guilds = discordClient.Guilds;
			foreach (var guild in guilds)
			{
				guild?.DownloadUsersAsync();
			}
			var guildUsers = guilds.Select(guild => guild?.Users);
			var result = new List<SocketGuildUser>();

			foreach (var guild in guildUsers)
			{
				foreach (var user in guild)
				{
					var preferredName = user.Nickname;
					if (user.Nickname == null)
						preferredName = user.Username;
					if(name.Equals(preferredName) || (!onlyExactMatch && (name.Contains(preferredName) || preferredName.Contains(name))))
					{
						result.Add(user);
					}
				}
			}
			
			return result;
		}

		internal static IEnumerable<SocketRole> GetRolesByName(string name, bool onlyExactMatch = false)
		{
			var discordClient = Discord.GetDiscord().GetAwaiter().GetResult();
			var rolesPerGuild = discordClient.Guilds.Select(guild => guild.Roles.Where(role => !onlyExactMatch ?
				name.Contains(role.Name) || role.Name.Contains(name) : name.Equals(role.Name)));
			
			var result = new List<SocketRole>();
			foreach (var roles in rolesPerGuild)
			{
				result.AddRange(roles);
			}

			return result;
		}

		internal static IEnumerable<SocketChannel> GetChannelsByName(string name, bool onlyExactMatch = false)
		{
			var discordClient = Discord.GetDiscord().GetAwaiter().GetResult();
			var channelsPerGuild = discordClient.Guilds.Select(guild => guild.Channels.Where(channel => !onlyExactMatch ?
				name.Contains(channel.Name) || channel.Name.Contains(name) : name.Equals(channel.Name)));

			var result = new List<SocketChannel>();
			foreach (var channels in channelsPerGuild)
			{
				result.AddRange(channels);
			}

			return result;
		}
	}
}
