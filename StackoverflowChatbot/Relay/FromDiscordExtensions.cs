using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Botler.Core.Config;
using Botler.Database;
using Botler.Database.Dbos;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace StackoverflowChatbot.Relay
{
	internal static class FromDiscordExtensions
	{
		internal static List<string> BuildSoMessage(SocketGuildUser user, Base config, SocketMessage arg, SqliteContext context)
        {
			var displayname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
            var messageStart = $@"\[**[{displayname}]({config.DiscordInviteLink})**] ";
			var messageContent = arg.Content;
			var result = new List<string>();

			if (arg.Reference != null) // reference = reply
			{
				var replyContextDbo = GetDboForReplyContextId(arg.Reference.MessageId.Value, context);

				if (replyContextDbo != null )
				{
					string stackMessageId = "0";
					if (replyContextDbo.OriginPlatform == MessageOriginDestination.StackOverflowChat)
					{
						stackMessageId = replyContextDbo.OriginMessageId;
					}else if (replyContextDbo.DestinationPlatform == MessageOriginDestination.StackOverflowChat)
					{
						stackMessageId = replyContextDbo.DestinationMessageId;
					}

					messageStart = $":{stackMessageId} " + messageStart;
				}
			}


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
			if (embeddedCode.Count == 0) result.Add(messageStart + messageContent);
			else
			{
				// Complex message with codeblocks...
				var cursor = 0;
				result.Add(messageStart);
				foreach (Match? codeBlock in embeddedCode)
				{
					if (codeBlock == null)
						continue;

					var indicatorCount = Regex.Matches(messageContent.Substring(0, codeBlock.Index + 3), "```", RegexOptions.Singleline).Count;
					if (indicatorCount % 2 == 0)
						continue;

					var soCodeBlock = "    " + codeBlock.ToString().Replace("`", "").Replace("\n", "\n    ").TrimEnd();
					result.Add(messageContent.Substring(cursor, codeBlock.Index - cursor));
					cursor = codeBlock.Index + codeBlock.Length;
					result.Add(soCodeBlock);
				}
			}

			//Add attachment links if it's a picture as a seperate message for stack. Specifically do it last
			foreach (var attachment in arg.Attachments)
			{
				result.Add(attachment.Url);
			}

			return result;
		}

		private static MessageDbo? GetDboForReplyContextId(ulong discordMessageId, SqliteContext context)
		{
			return context.Messages
				.Where(dbo =>
					(dbo.OriginPlatform == MessageOriginDestination.Discord && dbo.OriginMessageId == discordMessageId.ToString())
					|| (dbo.DestinationPlatform == MessageOriginDestination.Discord && dbo.DestinationMessageId == discordMessageId.ToString())
				)
				.SingleOrDefault();
		}
	}
}
