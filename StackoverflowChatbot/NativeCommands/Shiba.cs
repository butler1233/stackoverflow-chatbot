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
	/// Returns a random photo of a shiba inu dog.
	/// </summary>
	[UsedImplicitly]
	internal class Shiba: BaseCommand
	{
		private readonly ICommandProcessor _commandProcessor;
		private readonly ICommandFactory _commandFactory;

		public Help(ICommandProcessor commandProcessor, ICommandFactory commandFactory)
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
					HttpResponseMessage response = await client.GetAsync(url);

					if (response.IsSuccessStatusCode)
					{
						var response = await response.Content.ReadAsStringAsync();
						var shibaUrl = parseResponse(response);
						if (shibaUrl == null)
						{
							botResponse = "Unable to parse shibe response: " + response;
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

			return response.Substring(startIndex, endIndex - startIndex);
		}

		internal override string CommandName() => "shiba";

		internal override string? CommandDescription() => "Displays a random photo of a shiba inu dog.";
	}
}
