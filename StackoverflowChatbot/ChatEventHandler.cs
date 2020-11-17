using System;
using System.Web;
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

			if (!Config.Manager.Config().IgnoredUsers.Contains(chatEvent.UserId))
			{

				//Check if we're on Discord.
				var config = Config.Manager.Config();
				if (config.StackToDiscordMap.ContainsKey(chatEvent.RoomId))
				{
					//Tidy the message up
					var nohtml = HttpUtility.HtmlDecode(chatEvent.Content);
					//Spoiler onbox content so we don't kill everyone with spam.
					if (nohtml.StartsWith("<div class=\"onebox"))
					{
						nohtml = $"Onebox content! ||{nohtml}||";
					}



					var finalMessage = nohtml;
					//SEND INTO DISCORD
					var newmessage = $"[**[{chatEvent.Username}](https://chat.stackoverflow.com/transcript/message/{chatEvent.MessageId}#{chatEvent.MessageId})**] {finalMessage}";
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
}
