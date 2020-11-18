using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Discord;

namespace StackoverflowChatbot.Relay
{
	internal static class FromStackExtensions
	{
		private static readonly Regex obImageMatch = new Regex("<img src=\"(.+)(?= \" class)");
		private static readonly Regex obXkcdMatch = new Regex("<a rel=\"[^\"]+\" href=\"([^\"]+)\"><img src=\"([^\"]+)\" title=\"([^\"]+)\" alt=");
		internal static DiscordMessageModel OneboxImages(this DiscordMessageModel model)
		{
			//Try normla images
			var embed = new EmbedBuilder();
			var match = obImageMatch.Match(model.MessageContent);
			if (match.Success)
			{

				var imguri = match.Groups[1].Value;
				if (imguri.StartsWith("//")) imguri = $"https:" + imguri;
				embed.ImageUrl = imguri;

				model.Embed = embed.Build();
				model.MessageContent = "";
				return model;
			}

			//Try XKCD
			var xkcd = obXkcdMatch.Match(model.MessageContent);
			if (xkcd.Success)
			{
				embed.ImageUrl = match.Groups[2].Value;
				embed.Footer.Text = match.Groups[3].Value;
				embed.Url = match.Groups[1].Value;
				model.MessageContent = "";
				model.Embed = embed.Build();
				return model;
			}

			//No matches, return as is.

			return model;
		}

	}
}
