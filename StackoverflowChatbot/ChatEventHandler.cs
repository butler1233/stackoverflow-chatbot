using System;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.Extensions;
using StackoverflowChatbot.Relay;

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
						message = FromStackExtensions.MakePingsGreatAgain(message, discordClient);
						var newMessage = $"[**{chatEvent.Username}**] {message}";
						await textChannel.SendMessageAsync(newMessage);
					}
				}

				if (chatEvent.ContainsTrigger()) this.OnEvent(EventData.FromJson(data));
			}
		}
    }
}