using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SharpExchange.Chat.Actions;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;
using StackoverflowChatbot.Database;
using StackoverflowChatbot.Database.Dbos;
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
			client.MessageUpdated += ClientMessageUpdated;
			//Logs in
			await client.LoginAsync(TokenType.Bot, Config.Manager.Config().DiscordToken);
			await client.StartAsync();
			//Now wr're done
			return client;
		}

		private static async Task ClientMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
		{
			if (arg2.Author is SocketGuildUser user)
			{
				if (arg2.Author.IsBot) return;

				var config = Config.Manager.Config();
				Console.WriteLine($"[DIS EDIT {arg2.Channel.Name}] {arg2.Content}");
				//Check if we have a mapping.
				if (config.DiscordToStackMap.ContainsKey(arg2.Channel.Name))
				{
					//We are setup to map this channel's messages to stack.
					var roomId = config.DiscordToStackMap[arg2.Channel.Name];
					//Build the message
					//var displayname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;

					// Create a list of messages in case there are embedded codeblocks or stuff alongside text
					var messages = FromDiscordExtensions.BuildSoMessage(user, config, arg2);

					await SendEditToStack(messages.First(), roomId, arg2);
				}
			}
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

					await SendMessageToStack(messages, roomId, arg);
				}
			}
		}

		private static async Task SendMessageToStack(List<string> messages, int roomId, SocketMessage arg)
		{
			//this is terrible lol
			var dbc = new SqliteContext();

			var stackMessageIds = new List<int>();
			MessageDbo dbo = CreateFromChatEvent(arg);
			foreach (var message in messages)
			{
				//Find the room scheduler
				if (StackSchedulers.ContainsKey(roomId))
				{
					//We already have a scheduler, lets goooo
					var sched = StackSchedulers[roomId];
					stackMessageIds.Add(await sched.CreateMessageAsync(message));
				}
				//Or create one if we already have a watcher.
				else if (StackRoomWatchers.ContainsKey(roomId))
				{
					var watcher = StackRoomWatchers[roomId];
					var newScheduler = new ActionScheduler(watcher.Auth, RoomService.Host, roomId);
					StackSchedulers.Add(roomId, newScheduler);
					stackMessageIds.Add(await newScheduler.CreateMessageAsync(message));

					await arg.Channel.SendMessageAsync("Opened a new scheduler for sending messages to Stack. FYI.");
				}
				else
				{
					//or complain about not watching stack.
					await arg.Channel.SendMessageAsync(
						"Unable to sync messages to Stack - I'm not watching the corresponding channel. Invite me to the channel on stack and tryagain.");
				}
			}

			await UpdateWithStackDetails(dbo, stackMessageIds.First().ToString(), roomId.ToString(), dbc);

		}

		private static async Task SendEditToStack(string message, int roomId, SocketMessage arg)
		{
			//this is terrible lol
			var dbc = new SqliteContext();

			var dbo = await GetDboForReplyContextId(arg.Id, dbc);
			if (dbo == null)
			{
				return;
			}

			string? stackIdString = GetNonDiscordIdForDbo(dbo);
			if (stackIdString == null)
			{
				return;
			}

			int stackMessageId = int.Parse(stackIdString);
			bool updateSuccess = false;

			//Find the room scheduler
			if (StackSchedulers.ContainsKey(roomId))
			{
				//We already have a scheduler, lets goooo
				var sched = StackSchedulers[roomId];
				updateSuccess = await sched.EditMessageAsync(stackMessageId, message);
			}
			//Or create one if we already have a watcher.
			else if (StackRoomWatchers.ContainsKey(roomId))
			{
				var watcher = StackRoomWatchers[roomId];
				var newScheduler = new ActionScheduler(watcher.Auth, RoomService.Host, roomId);
				StackSchedulers.Add(roomId, newScheduler);
				updateSuccess = await newScheduler.EditMessageAsync(stackMessageId, message);

				await arg.Channel.SendMessageAsync("Opened a new scheduler for sending messages to Stack. FYI.");
			}
			else
			{
				//or complain about not watching stack.
				await arg.Channel.SendMessageAsync(
					"Unable to sync messages to Stack - I'm not watching the corresponding channel. Invite me to the channel on stack and tryagain.");
			}

			dbo.IsEdited = true;
			dbo.MessageBody = message;
			await dbc.SaveChangesAsync();

			if (!updateSuccess)
			{
				await arg.AddReactionAsync(new Emoji("☠"));
			}

		}



		private static MessageDbo CreateFromChatEvent(SocketMessage eventData)
		{
			var dbo = new MessageDbo
			{
				OriginPlatform = MessageOriginDestination.Discord,
				OriginAuthor = eventData.Author.Username,
				OriginMessageId = eventData.Id.ToString(),
				OriginChannelId = eventData.Channel.Id.ToString(),
				MessageBody = eventData.Content
			};
			return dbo;
		}

		private static async Task UpdateWithStackDetails(MessageDbo dbo, string stackMessageId, string stackRoomId, SqliteContext context)
		{
			dbo.DestinationMessageId = stackMessageId;
			dbo.DestinationPlatform = MessageOriginDestination.StackOverflowChat;
			dbo.DestinationChannelId = stackRoomId;

			await context.Messages.AddAsync(dbo);
			await context.SaveChangesAsync();
		}

		private static async Task<MessageDbo?> GetDboFromDiscordMessageId(int stackMessageId, SqliteContext context)
		{
			return await context.Messages.SingleOrDefaultAsync(dbo =>
				dbo.OriginPlatform == MessageOriginDestination.StackOverflowChat
				&& dbo.OriginMessageId == stackMessageId.ToString()
			);
		}

		/// <summary>
		/// Hopefully gets the dbo which corresponds to the message a stack reply is replying to. It could be both stack messages, or it could be a discord message posted by the bot, so it has to check both.
		/// </summary>
		/// <param name="stackMessageId"></param>
		/// <returns></returns>
		private static async Task<MessageDbo?> GetDboForReplyContextId(ulong stackMessageId, SqliteContext context)
		{
			return await context.Messages
				.Where(dbo =>
					(dbo.OriginPlatform == MessageOriginDestination.Discord && dbo.OriginMessageId == stackMessageId.ToString())
					|| (dbo.DestinationPlatform == MessageOriginDestination.Discord && dbo.DestinationMessageId == stackMessageId.ToString())
				)
				.SingleOrDefaultAsync();
		}

		private static string? GetNonDiscordIdForDbo(MessageDbo dbo)
		{
			if (dbo.OriginPlatform == MessageOriginDestination.Discord)
			{
				return dbo.DestinationMessageId;
			}
			else if (dbo.DestinationPlatform == MessageOriginDestination.Discord)
			{
				return dbo.OriginMessageId;
			}
			else
			{
				return null;
			}
		}


		internal static List<SocketGuildUser> GetUserByName(string name, bool onlyExactMatch = false)
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

		internal static List<SocketRole> GetRolesByName(string name, bool onlyExactMatch = false)
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
