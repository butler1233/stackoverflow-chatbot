using System;
using System.Text.RegularExpressions;
using System.Web;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.Extensions;
using StackoverflowChatbot.Relay;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler: ChatEventDataProcessor
	{
		public event Action<EventData> OnEvent = null!;

		public override EventType[] Events { get; } = { EventType.MessagePosted, EventType.MessageEdited };

		public Regex multilineCodeRegex = new Regex("(?:<pre)(?: class='full')?>(.+)(?=</pre>)", RegexOptions.Singleline);


		public override async void ProcessEventData(EventType eventType, JToken data)
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
						nohtml = $"Onebox content! ||```html\r\n{nohtml}```||";
					}
					//Try to match the multiline code
					var match = this.multilineCodeRegex.Match(nohtml);
					if (match.Success)
					{
						nohtml = $"\r\n```\r\n{match.Groups[1]}```";
					}

					var finalMessage = nohtml;
					//SEND INTO DISCORD
					
					var channelName = config.StackToDiscordMap[chatEvent.RoomId];
					var discordClient = await Discord.GetDiscord();
					var discord = discordClient.GetChannel(config.DiscordChannelNamesToIds[channelName]);
					if (discord is SocketTextChannel textChannel)
					{
						var model = new DiscordMessageModel{MessageContent = finalMessage};
						model = model.OneboxImages();
						var newmessage = $"[**{chatEvent.Username}**] {model.MessageContent}";
						await textChannel.SendMessageAsync(newmessage, false, model.Embed);
					}

				}

				if (!chatEvent.ContainsTrigger()) return;

				this.OnEvent?.Invoke(EventData.FromJson(data));

			}

			
		}

	}
}
