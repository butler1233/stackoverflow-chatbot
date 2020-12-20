using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace StackoverflowChatbot.Relay
{
	internal static class FromDiscordExtensions
	{
		internal static List<string> BuildSoMessage(SocketGuildUser user, Config.Base config, SocketMessage arg)
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
			var cursor = 0;
			result.Add(messageStart);
			foreach (Match? codeBlock in embeddedCode)
			{
				if (codeBlock == null)
					continue;

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
