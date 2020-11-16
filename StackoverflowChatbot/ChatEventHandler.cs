using System;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.Extensions;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler: ChatEventDataProcessor
	{
		public event Action<EventData> OnEvent = null!;

		public override EventType[] Events { get; } = { EventType.MessagePosted, EventType.MessageEdited };

		public override void ProcessEventData(EventType eventType, JToken data)
		{
			var chatEvent = EventData.FromJson(data);

			//Check if we're on Discord.
			var config = Config.Manager.Config();
			if (config.StackToDiscordMap.ContainsKey(chatEvent.RoomId))
			{
				//SEND INTO DISCORD
				var newmessage = $"[**{chatEvent.Username}** *on SO*] {chatEvent.Content}";
				var channelName = config.StackToDiscordMap[chatEvent.RoomId];
				var discord = Discord.GetDiscord().GetChannel(config.DiscordChannelNamesToIds[channelName]);
				if (discord is SocketTextChannel textChannel)
				{
					textChannel.SendMessageAsync(newmessage);
				}

			}

			if (!chatEvent.ContainsTrigger()) return;

			this.OnEvent?.Invoke(EventData.FromJson(data));
		}

	}
}
