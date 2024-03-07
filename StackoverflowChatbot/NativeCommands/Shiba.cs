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
		private readonly ICommandProcessor _commandProcessor;
		private readonly ICommandFactory _commandFactory;

		public Shiba(ICommandProcessor commandProcessor, ICommandFactory commandFactory)
		{
			_commandProcessor = commandProcessor;
			_commandFactory = commandFactory;
		}

		internal override IAction? ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters)
		{
			var url = "https://shibe.online/api/shibes";
			string botResponse;

			using (HttpClient client = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = client.GetAsync(url).Result;

					if (response.IsSuccessStatusCode)
					{
						var responseBody = response.Content.ReadAsStringAsync().Result;
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
		internal string parseResponse(string response)
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

			return response.Substring(start, end - start);
		}

		internal override string CommandName() => "shiba";

		internal override string? CommandDescription() => "Displays a random photo of a shiba inu dog. Photos from https://shibe.online";
	}
}
