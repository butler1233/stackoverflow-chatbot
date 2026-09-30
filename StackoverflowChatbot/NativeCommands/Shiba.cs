using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;
using StackoverflowChatbot.CommandProcessors;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Returns a random photo of a shiba inu dog. Photos from https://shibe.online.
	/// </summary>
	[UsedImplicitly]
	internal class Shiba: BaseCommand
	{
		internal override async Task<IAction?> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters)
		{
			var url = "https://shibe.online/api/shibes";
			string botResponse;

			using (var client = new HttpClient())
			{
				try
				{
					var response = await client.GetAsync(url);

					if (response.IsSuccessStatusCode)
					{
						var responseBody = await response.Content.ReadAsStringAsync();
						var shibaUrl = parseResponse(responseBody);
						if (shibaUrl == null)
						{
	  						botResponse = "Unable to parse shibe response: " + responseBody;
						}
						else
						{
							botResponse = shibaUrl;
						}
					}
					else
					{
						botResponse = "Error getting shibe: HTTP " + response.StatusCode;
					}
				}
				catch (Exception ex)
				{
					botResponse = "Error getting shibe: " + ex.Message;
				}
			}

			return new SendMessage(botResponse);
		}

		//example response:
		//["https://cdn.shibe.online/shibes/0ce15f51b543ceb8a0387f3428e9ecce24499967.jpg"]
		internal string? parseResponse(string response)
		{
			var start = response.IndexOf("https://");
			if (start < 0)
			{
				return null;
			}

			var end = response.IndexOf("\"]", start);
			if (end < 0)
			{
				return null;
			}

			return response[start..end];
		}

		internal override string CommandName() => "shiba";

		internal override string? CommandDescription() => "Displays a random photo of a shiba inu dog. Photos from https://shibe.online";
	}
}
