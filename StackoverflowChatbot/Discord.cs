using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SharpExchange.Chat.Actions;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;

namespace StackoverflowChatbot
{
	internal static class Discord
	{
		private static DiscordSocketClient _client = null;

		internal static async Task<DiscordSocketClient> GetDiscord()
		{
			if (_client == null)
			{
				_client = await CreateDiscordClient();
			}

			return _client;
		}

		internal static Dictionary<int,RoomWatcher<DefaultWebSocket>> StackRoomWatchers = new Dictionary<int, RoomWatcher<DefaultWebSocket>>();
		internal static Dictionary<int,ActionScheduler> StackSchedulers = new Dictionary<int, ActionScheduler>();

		private static async Task<DiscordSocketClient> CreateDiscordClient()
		{
			var client = new DiscordSocketClient();
			//Setuo handlers
			client.MessageReceived += ClientRecieved;
			//Logs in
			await client.LoginAsync(TokenType.Bot, Config.Manager.Config().DiscordToken);
			await client.StartAsync();
			//Now wr're done
			return client;
		}

		private static Task ClientRecieved(SocketMessage arg)
		{

			if (arg.Author is SocketGuildUser user)
			{
				if (arg.Author.IsBot) return Task.CompletedTask;
				var config = Config.Manager.Config();
				Console.WriteLine($"[DIS {arg.Channel.Name}] {arg.Content}");
				//Check if we have a mapping.
				if (config.DiscordToStackMap.ContainsKey(arg.Channel.Name))
				{
					//We are setup to map this channel's messages to stack.
					var roomId = config.DiscordToStackMap[arg.Channel.Name];
					//Build the message
					var displayname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;

					// Create a list of messages in case there are embedded codeblocks or stuff alongside text
					var messages = BuildSoMessage(user, config, arg);
					
					foreach (var message in messages)
					{
						//Find the room scheduler
						if (StackSchedulers.ContainsKey(roomId))
						{
							//We already have a scheduler, lets goooo
							var sched = StackSchedulers[roomId];
							sched.CreateMessageAsync(message);
						}
						//Or create one if we already have a watcher.
						else if (StackRoomWatchers.ContainsKey(roomId))
						{
							var watcher = StackRoomWatchers[roomId];
							var newScheduler = new ActionScheduler(watcher.Auth, RoomService.Host, roomId);
							StackSchedulers.Add(roomId, newScheduler);
							newScheduler.CreateMessageAsync(message);
							
							arg.Channel.SendMessageAsync("Opened a new scheduler for sending messages to Stack. FYI.");
						}
						else
						{
							//or complain about not watching stack.
							arg.Channel.SendMessageAsync(
								"Unable to sync messages to Stack - I'm not watching the corresponding channel. Invite me to the channel on stack and tryagain.");
							return Task.CompletedTask;
						}
					}
				}
				//Nothing to do, who cares
				return Task.CompletedTask;
			}

			return Task.CompletedTask;
		}

        private static List<string> BuildSoMessage(SocketGuildUser user, Config.Base config, SocketMessage arg)
        {
			var displayname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
            var messageStart = $@"\[**[{displayname}]({config.DiscordInviteLink})**] ";
			var messageContent = arg.Content;
			var result = new List<string>();

			foreach (var mentionedUser in arg.MentionedUsers)
			{
				messageContent = messageContent.Replace(mentionedUser.Mention, $"@{mentionedUser.Username}");
			}
			foreach (var mentionedRoles in arg.MentionedRoles)
			{
				messageContent = messageContent.Replace(mentionedRoles.Mention, $"[@{mentionedRoles.Name}]({config.DiscordInviteLink})");
			}
			foreach (var mentionedChannel in arg.MentionedChannels)
			{
				// Library doesn't provide channel mention string
				messageContent = messageContent.Replace($"<#{mentionedChannel.Id}>", $"[#{mentionedChannel.Name}]({config.DiscordInviteLink})");
			}
			
			var embeddedCode = Regex.Matches(messageContent, "```.+?```", RegexOptions.Singleline);
			if (embeddedCode.Count == 0)
				return new List<string>() { messageStart + messageContent };

			// Complex message with codeblocks...
			int cursor = 0;
			result.Add(messageStart);
			foreach (Match codeBlock in embeddedCode)
			{
				var indicatorCount = Regex.Matches(messageContent.Substring(0, codeBlock.Index + 3), "```", RegexOptions.Singleline).Count;
				if(indicatorCount % 2 == 0)
					continue;

				var soCodeBlock = "    " + codeBlock.ToString().Replace("`", "").Replace("\n", "\n    ").TrimEnd();
				result.Add(messageContent.Substring(cursor, codeBlock.Index - cursor));
				cursor = codeBlock.Index + codeBlock.Length;
				result.Add(soCodeBlock);
			}

			return result;
        }
    }
}
