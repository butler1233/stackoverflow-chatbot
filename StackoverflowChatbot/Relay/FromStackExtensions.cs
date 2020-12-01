using System;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using Discord.WebSocket;

namespace StackoverflowChatbot.Relay
{
	internal static class FromStackExtensions
	{
		internal static string ProcessStackMessage(this string message, int roomId, string roomName)
		{
			var baseUri = new Uri($"https://chat.stackoverflow.com/rooms/{roomId}/{Uri.EscapeUriString(roomName)}");
			var document = new HtmlDocument();
			document.LoadHtml(message);
			var documentNode = document.DocumentNode;

			// Handle code-blocks.
			if (documentNode.FirstChild.Name.Equals("pre"))
			{
				var text = HtmlEntity.DeEntitize(document.DocumentNode.InnerText);
				return $"```\r\n{text}\r\n```";
			}

			// Handle onebox-content.
			var classes = documentNode.FirstChild.GetAttributeValue("class", string.Empty).Split(' ')
				.Select(x => x.ToLowerInvariant())
				.ToHashSet();
			if (classes.Contains("ob-image"))
			{
				var img = document.DocumentNode.SelectSingleNode("//img");
				var src = HtmlEntity.DeEntitize(img?.GetAttributeValue("src", string.Empty));
				if (Uri.TryCreate(baseUri, src, out var result)) return result.ToString();
			}
			else if (classes.Contains("ob-xkcd") || classes.Contains("ob-youtube") || classes.Contains("ob-message"))
			{
				var anchor = document.DocumentNode.SelectSingleNode("//a");
				var href = HtmlEntity.DeEntitize(anchor?.GetAttributeValue("href", string.Empty));
				if (Uri.TryCreate(baseUri, href, out var result)) return result.ToString();
			}

			// Handle (multiline) text.
			using var writer = new StringWriter();
			ConvertTo(document.DocumentNode, writer);
			writer.Flush();
			return writer.ToString();
		}

		private static void ConvertTo(HtmlNode node, TextWriter writer)
		{
			switch (node.NodeType)
			{
				case HtmlNodeType.Document:
					if (node.HasChildNodes)
						foreach (var childNode in node.ChildNodes)
							ConvertTo(childNode, writer);
					break;
				case HtmlNodeType.Element:
					if (node.Name.Equals("code"))
						writer.Write("`");
					if (node.HasChildNodes)
						foreach (var childNode in node.ChildNodes)
							ConvertTo(childNode, writer);
					if (node.Name.Equals("br") || node.Name.Equals("p"))
						writer.Write("\r\n");
					if (node.Name.Equals("code"))
						writer.Write("` ");
					break;
				case HtmlNodeType.Text:
					if (node.InnerText.TrimStart().Length == 0) break;
					writer.Write(HtmlEntity.DeEntitize(node.InnerText.TrimStart()));
					break;
			}
		}

		
        internal static string MakePingsGreatAgain(string message, DiscordSocketClient discordClient, bool inCaseMultipleFoundUseFirst = true)
        {
            var pings = Regex.Matches(message, @"[@#][^\s]+");
			foreach (Match possiblePing in pings)
			{
				if (possiblePing.ToString()[0] == '@')
				{
					var pingString = possiblePing.ToString().Replace("@", "");
					var possibleUsers = Discord.GetUserByName(pingString);
					if (possibleUsers.Count() == 1 || (inCaseMultipleFoundUseFirst && possibleUsers.Count() > 1))
					{
						message = message.Replace(possiblePing.ToString(), possibleUsers.First().Mention);
					}
					else if (possibleUsers.Count() == 0)
					{
						// Role ping UwU
						var possibleRoles = Discord.GetRolesByName(pingString);
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
	}
}