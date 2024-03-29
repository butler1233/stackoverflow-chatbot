using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grpc.Core;
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
					await ProcessEditMessage(data);
					break;
				case EventType.MessageStarToggled:
					await ProcessMessageStarred(data);
					break;
				// Ones that we don't care enough even to log
				case EventType.UserEntered:
				case EventType.UserLeft:
				case EventType.MessageReply: //The data is mostly duplicate of MessagePosted
					break;
				// The rest
				default:
					DumpToLog(eventType, data);
					break;

			}
		}

		private void DumpToLog(EventType type, JToken data) => Console.WriteLine($"Message type [{type}]: {JsonConvert.SerializeObject(data)}");

		private async Task ProcessNewMessage(JToken data)
		{
			var chatEvent = ChatMessageEventData.FromJson(data);
			MessageDbo dbo = CreateFromChatEvent(chatEvent);

			if (!Config.Manager.Config().IgnoredUsers.Contains(chatEvent.UserId))
			{
				if (_config.StackToDiscordMap.ContainsKey(chatEvent.RoomId))
				{

					//check if it's a reply, and if it is, dig up the discord message it's replying to, if we have it.
					MessageReference replyReference = null;
					if (chatEvent.IsReply)
					{
						int replyingTo = chatEvent.ReplyingToId;
						MessageDbo? replyContext = await GetDboForReplyContextId(replyingTo);
						if (replyContext != null && (replyContext.OriginPlatform == MessageOriginDestination.Discord || replyContext.DestinationPlatform == MessageOriginDestination.Discord))
						{
							string? discordMessageId = GetNonStackMessageIdForDbo(replyContext);
							if (discordMessageId != null)
							{
								replyReference = new MessageReference(ulong.Parse(discordMessageId));
							}
						}
					}

					//Send message as normal
					var channelName = _config.StackToDiscordMap[chatEvent.RoomId];
					var discordClient = await Discord.GetDiscord();
					var discord = discordClient.GetChannel(_config.DiscordChannelNamesToIds[channelName]);
					if (discord is SocketTextChannel textChannel)
					{
						var message = chatEvent.Content.ProcessStackMessage(chatEvent.RoomId, chatEvent.RoomName);
						message = FromStackExtensions.MakePingsGreatAgain(message);
						var newMessage = $"[**{chatEvent.Username}**] {message}";
						RestUserMessage? discordMesasge = await textChannel.SendMessageAsync(newMessage, messageReference:replyReference);
						await UpdateWithDiscordDetails(dbo, discordMesasge.Id.ToString(), discordMesasge.Channel.Id.ToString());

					}
				}

				if (chatEvent.ContainsTrigger()) OnEvent(ChatMessageEventData.FromJson(data));
			}
		}

		private async Task ProcessEditMessage(JToken data)
		{
			var chatEvent = ChatMessageEventData.FromJson(data);
			var dbo = await GetDboFromStackMessageId(chatEvent.MessageId);
			if (dbo != null)
			{
				if (dbo.DestinationPlatform == MessageOriginDestination.Discord)
				{
					DiscordSocketClient discord = await Discord.GetDiscord();
					SocketChannel channel = discord.GetChannel(ulong.Parse(dbo.DestinationChannelId));
					if (channel != null && channel is SocketTextChannel textChannel)
					{
						var message = await textChannel.GetMessageAsync(ulong.Parse(dbo.DestinationMessageId));

						await textChannel.ModifyMessageAsync(ulong.Parse(dbo.DestinationMessageId), properties =>
						{
							var message = chatEvent.Content.ProcessStackMessage(chatEvent.RoomId, chatEvent.RoomName);
							message = FromStackExtensions.MakePingsGreatAgain(message);
							var newMessage = $"[**{chatEvent.Username}**] {message}";
							properties.Content = newMessage;
						});
						if (!dbo.IsEdited)
						{
							await message.AddReactionAsync(new Emoji("✏"), RequestOptions.Default);
						}
						
						dbo.IsEdited = true;
						dbo.MessageBody = chatEvent.Content;
						await _sqliteContext.SaveChangesAsync();
					}
				}
			}
		}

		private async Task ProcessMessageStarred(JToken data)
		{
			var eventData = MessageStarredEventData.FromJson(data);
			var dbo = await GetDboFromStackMessageId(eventData.MessageId);
			if (dbo != null)
			{
				if (dbo.DestinationPlatform == MessageOriginDestination.Discord)
				{
					DiscordSocketClient discord = await Discord.GetDiscord();
					SocketChannel channel = discord.GetChannel(ulong.Parse(dbo.DestinationChannelId));
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
				OriginChannelId = eventData.RoomId.ToString(),
				MessageBody = eventData.Content
			};
			return dbo;
		}

		private async Task UpdateWithDiscordDetails(MessageDbo dbo, string discordMessageId, string discordChannelId)
		{
			dbo.DestinationMessageId = discordMessageId;
			dbo.DestinationPlatform = MessageOriginDestination.Discord;
			dbo.DestinationChannelId = discordChannelId;
			
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

		/// <summary>
		/// Hopefully gets the dbo which corresponds to the message a stack reply is replying to. It could be both stack messages, or it could be a discord message posted by the bot, so it has to check both.
		/// </summary>
		/// <param name="stackMessageId"></param>
		/// <returns></returns>
		private async Task<MessageDbo?> GetDboForReplyContextId(int stackMessageId)
		{
			return await _sqliteContext.Messages
				.Where(dbo =>
					(dbo.OriginPlatform == MessageOriginDestination.StackOverflowChat && dbo.OriginMessageId == stackMessageId.ToString())
					|| (dbo.DestinationPlatform == MessageOriginDestination.StackOverflowChat && dbo.DestinationMessageId == stackMessageId.ToString())
				)
				.SingleOrDefaultAsync();
		}

		private string? GetNonStackMessageIdForDbo(MessageDbo dbo)
		{
			if (dbo.OriginPlatform == MessageOriginDestination.StackOverflowChat)
			{
				return dbo.DestinationMessageId;
			}
			else if (dbo.DestinationPlatform == MessageOriginDestination.StackOverflowChat)
			{
				return dbo.OriginMessageId;
			}
			else
			{
				return null;
			}
		}
    }
}