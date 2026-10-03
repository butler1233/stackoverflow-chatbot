using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	internal class Weather: BaseCommand
	{
		private readonly IHttpService _httpService;

		public Weather(IHttpService httpService) => _httpService = httpService;

		internal override string? CommandDescription() => "Returns the current weather for a given location. Usage: !weather <location>";
		internal override string CommandName() => "weather";
		internal override async Task<IAction?> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) =>
			new SendMessage(
				await _httpService.Get<string>(
					$"https://wttr.in/~{System.Web.HttpUtility.UrlEncode(eventContext.CommandParameters)}?format=3"
				));
	}
}
