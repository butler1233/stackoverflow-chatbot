using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StackoverflowChatbot.Services;
using System.Net.Http;
using System.Web;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Returns information about this bot.
	/// </summary>
	[UsedImplicitly]
	internal class TTS: BaseCommand
	{
        private readonly Dictionary<string, string> _languageApis = new Dictionary<string, string> { 
            { "en", "http://5.189.153.146:5002/api/tts?text=" },
            { "de", "http://5.189.153.146:5003/api/tts?text=" }
        }; 

		internal override IAction ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) 
        {
            if (parameters == null || parameters.Length == 0)
                return new SendMessage("you need to provide something that can be read");

            var language = "en";
            var saneText = string.Empty;
        
            if (_languageApis.ContainsKey(parameters[0]))
                language = parameters[0];
            else 
                saneText += parameters[0];

            for (int i = 1; i < parameters.Length; i++)
            {
                saneText += " " + parameters[i];
            }
            saneText = saneText.Trim();

            var client = new HttpClient();
            var request = client.GetAsync(_languageApis[language] + HttpUtility.UrlEncode(saneText)).GetAwaiter().GetResult();
            var file = request.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

            var fileService = new htputFileService();
            var linkToAudio = fileService.UploadFileAsync(file, "<audio controls=\"controls\" autobuffer=\"autobuffer\" autoplay=\"autoplay\"><source src=\"data:audio/wav;base64,", "\"/></audio>").GetAwaiter().GetResult();
            return new SendMessage($"[**TTS**] [{saneText}]({linkToAudio})", $"[**TTS**] {linkToAudio}");
        }

		internal override string CommandName() => "tts";

		internal override string? CommandDescription() => "text to speech, languages en, de available, en is default.";
	}
}