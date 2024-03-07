using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;
using StackoverflowChatbot.CommandProcessors;

using System;
using System.Net.Http;
using System.Text.Json;
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
			string url = "https://shibe.online/api/shibes";

			using (HttpClient client = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(url);

					if (response.IsSuccessStatusCode)
					{
						using (var responseStream = await response.Content.ReadAsStreamAsync())
						{
							var jsonArray = await JsonSerializer.DeserializeAsync<string>(responseStream);
							return new SendMessage(jsonArray[0]);
						}
					}
					else
					{
						return new SendMessage("Error getting shibe: HTTP " + response.StatusCode);
					}
				}
				catch (Exception ex)
				{
					return new SendMessage("Error getting shibe: " + ex.Message);
				}
			}
		}

		internal override string CommandName() => "shiba";

		internal override string? CommandDescription() => "Displays a random photo of a shiba inu dog.";
	}
}
