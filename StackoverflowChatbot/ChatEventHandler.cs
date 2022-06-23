using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.ChatEvents.StackOverflow;
using StackoverflowChatbot.Config;
using StackoverflowChatbot.Database;
using StackoverflowChatbot.Database.Dbos;
using StackoverflowChatbot.Extensions;
using StackoverflowChatbot.Relay;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler : ChatEventDataProcessor
	{
		private readonly SqliteContext _sqliteContext;
		private readonly Base _config;

		public ChatEventHandler()
		{
			_sqliteContext = new SqliteContext();
			_config = Manager.Config();
		}

		public event Action<ChatMessageEventData> OnEvent = null!;

		public override EventType[] Events { get; } =
		{
			EventType.All
		};

		public override async void ProcessEventData(EventType eventType, JToken data)
		{
			switch (eventType)
			{
				case EventType.MessagePosted:
					await ProcessNewMessage(data);
					break;
				case EventType.MessageEdited:
					break;
				case EventType.MessageStarToggled:
					await ProcessMessageStarred(data);
					break;
				default:
					DumpToLog(eventType, data);
					break;

			}
		}

		private void DumpToLog(EventType type, JToken data) => Console.WriteLine($"Message type [{type}]: {JsonConvert.SerializeObject(data)}");

		private async Task ProcessNewMessage(JToken data)
		{
			var chatEvent = ChatMessageEventData.FromJson(data);
			var dbo = CreateFromChatEvent(chatEvent);
			DumpToLog(EventType.MessagePosted, data); //todo: remove

			if (!Config.Manager.Config().IgnoredUsers.Contains(chatEvent.UserId))
			{
				if (_config.StackToDiscordMap.ContainsKey(chatEvent.RoomId))
				{
					var channelName = _config.StackToDiscordMap[chatEvent.RoomId];
					var discordClient = await Discord.GetDiscord();
					var discord = discordClient.GetChannel(_config.DiscordChannelNamesToIds[channelName]);
					if (discord is SocketTextChannel textChannel)
					{
						var message = chatEvent.Content.ProcessStackMessage(chatEvent.RoomId, chatEvent.RoomName);
						message = FromStackExtensions.MakePingsGreatAgain(message);
						var newMessage = $"[**{chatEvent.Username}**] {message}";
						RestUserMessage? discordMesasge = await textChannel.SendMessageAsync(newMessage);
						await UpdateWithDiscordDetails(dbo, discordMesasge.Id.ToString());

					}
				}

				if (chatEvent.ContainsTrigger()) OnEvent(ChatMessageEventData.FromJson(data));
			}
		}

		private async Task ProcessMessageStarred(JToken data)
		{
			var eventData = MessageStarredEventData.FromJson(data);
			var dbo = await GetDboFromStackMessageId(eventData.MessageId);
			if (dbo != null)
			{
				if (dbo.DestinationPlatform == MessageOriginDestination.Discord && dbo.DestinationMessageId != null)
				{
					DiscordSocketClient discord = await Discord.GetDiscord();
					string channelName = _config.StackToDiscordMap[eventData.RoomId];
					SocketChannel channel = discord.GetChannel(_config.DiscordChannelNamesToIds[channelName]);
					if (channel != null && channel is SocketTextChannel textChannel)
					{
						var message = await textChannel.GetMessageAsync(ulong.Parse(dbo.DestinationMessageId));
						if (eventData.MessageStars > 0)
						{
							await message.AddReactionAsync(new Emoji("⭐"), RequestOptions.Default);
						}
						else
						{
							await message.RemoveReactionAsync(new Emoji("⭐"), discord.CurrentUser);
						}
						
					}
				}

				

			}
		}

		private MessageDbo CreateFromChatEvent(ChatMessageEventData eventData)
		{
			var dbo = new MessageDbo
			{
				OriginPlatform = MessageOriginDestination.StackOverflowChat,
				OriginAuthor = eventData.Username,
				OriginMessageId = eventData.MessageId.ToString(),
				MessageBody = eventData.Content
			};
			return dbo;
		}

		private async Task UpdateWithDiscordDetails(MessageDbo dbo, string discordMessageId)
		{
			dbo.DestinationMessageId = discordMessageId;
			dbo.DestinationPlatform = MessageOriginDestination.Discord;
			
			await _sqliteContext.Messages.AddAsync(dbo);
			await _sqliteContext.SaveChangesAsync();
		}

		private async Task<MessageDbo?> GetDboFromStackMessageId(int stackMessageId)
		{
			return await _sqliteContext.Messages.SingleOrDefaultAsync(dbo =>
				dbo.OriginPlatform == MessageOriginDestination.StackOverflowChat
				&& dbo.OriginMessageId == stackMessageId.ToString()
			);
		}
    }
}