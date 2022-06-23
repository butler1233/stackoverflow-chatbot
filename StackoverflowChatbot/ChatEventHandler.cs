using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.Database;
using StackoverflowChatbot.Database.Dbos;
using StackoverflowChatbot.Extensions;
using StackoverflowChatbot.Relay;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler : ChatEventDataProcessor
	{
		private readonly SqliteContext _sqliteContext;

		public ChatEventHandler()
		{
			_sqliteContext = new SqliteContext();
		}

		public event Action<EventData> OnEvent = null!;

		public override EventType[] Events { get; } =
		{
			EventType.All
		};

		public override async void ProcessEventData(EventType eventType, JToken data)
		{
			switch (eventType)
			{
				case EventType.MessagePosted:
				case EventType.MessageEdited:
					await ProcessNewMessage(data);
					break;
				default:
					DumpToLog(eventType, data);

			}
		}

		private void DumpToLog(EventType type, JToken data) => Console.WriteLine($"Message type [{type}]: {JsonConvert.SerializeObject(data)}");

		private async Task ProcessNewMessage(JToken data)
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
						message = FromStackExtensions.MakePingsGreatAgain(message);
						var newMessage = $"[**{chatEvent.Username}**] {message}";
						await textChannel.SendMessageAsync(newMessage);
					}
				}

				if (chatEvent.ContainsTrigger()) OnEvent(EventData.FromJson(data));
			}
		}

		private MessageDbo CreateOrUpdateFromChatEvent(EventData ecent)
		{
			if
		}
    }
}