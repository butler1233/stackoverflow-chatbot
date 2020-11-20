using System;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.Extensions;
using StackoverflowChatbot.Relay;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler : ChatEventDataProcessor
	{
		public event Action<EventData> OnEvent = null!;

		public override EventType[] Events { get; } = { EventType.MessagePosted, EventType.MessageEdited };

		public override async void ProcessEventData(EventType eventType, JToken data)
		{
			var chatEvent = EventData.FromJson(data);
			if (!Config.Manager.Config().IgnoredUsers.Contains(chatEvent.UserId))
			{
				var config = Config.Manager.Config();

				if (config.StackToDiscordMap.ContainsKey(chatEvent.RoomId))
				{
					var channelName = config.StackToDiscordMap[chatEvent.RoomId];
					var discordClient = await Discord.GetDiscord();
					var discord = discordClient.GetChannel(config.DiscordChannelNamesToIds[channelName]);
					if (discord is SocketTextChannel textChannel)
					{
						var message = chatEvent.Content.ProcessStackMessage(chatEvent.RoomId, chatEvent.RoomName);
						message = MakePingsGreatAgain(message, discordClient);
						var newMessage = $"[**{chatEvent.Username}**] {message}";
						await textChannel.SendMessageAsync(newMessage);
					}
				}

				if (chatEvent.ContainsTrigger()) this.OnEvent(EventData.FromJson(data));
			}
		}

        private string MakePingsGreatAgain(string message, DiscordSocketClient discordClient, bool inCaseMultipleFoundUseFirst = true)
        {
            var pings = Regex.Matches(message, @"[@#][^\s]+");
			foreach (Match possiblePing in pings)
			{
				if (possiblePing.ToString()[0] == '@')
				{
					var pingString = possiblePing.ToString().Replace("@", "");
					var possibleUsers = GetUserByName(pingString);
					if (possibleUsers.Count() == 1 || (inCaseMultipleFoundUseFirst && possibleUsers.Count() > 1))
					{
						message = message.Replace(possiblePing.ToString(), possibleUsers.First().Mention);
					}
					else if (possibleUsers.Count() == 0)
					{
						// Role ping UwU
						var possibleRoles = GetRolesByName(pingString);
						if (possibleRoles.Count() == 1 || (inCaseMultipleFoundUseFirst && possibleRoles.Count() > 1))
						{
							message = message.Replace(possiblePing.ToString(), possibleRoles.First().Mention);
						}
					}
				}
				else
				{
					// Channel mention, probably fine without doing anything
				}
			}

			return message;
        }

		private IEnumerable<SocketGuildUser> GetUserByName(string name, bool onlyExactMatch = false)
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

		private IEnumerable<SocketRole> GetRolesByName(string name, bool onlyExactMatch = false)
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

		private IEnumerable<SocketChannel> GetChannelsByName(string name, bool onlyExactMatch = false)
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