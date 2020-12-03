using System.Threading.Tasks;
using Discord.WebSocket;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.Config;

namespace StackoverflowChatbot.Actions
{
    internal class SendMessage : IAction
    {
        private readonly string _message;
        private readonly string? _markdownMessage;

        /// <summary>
        /// Creates a message for chat.so and discord
        /// </summary>
        /// <param name="message">the raw message used for chat.so and discord if markdownMessage is null</param>
        /// <param name="markdownMessage">(optional) a message with markdown formatting used for discord</param>
        public SendMessage(string message, string markdownMessage = null)
        {
            _message = message;
            _markdownMessage = markdownMessage;
        }
        public async Task Execute(ActionScheduler scheduler)
        {
            // Send message to chat.so
            await scheduler.CreateMessageAsync(_message);

            // Send message to Discord
            var config = Manager.Config();
            var channelName = config.StackToDiscordMap[scheduler.RoomId];
            var discordClient = await Discord.GetDiscord();
            var discord = discordClient.GetChannel(config.DiscordChannelNamesToIds[channelName]);

            if (discord is SocketTextChannel textChannel)
            {
                if (_markdownMessage != null)
                {
                    await textChannel.SendMessageAsync($"{_markdownMessage}");
                }
                else
                {
				    await textChannel.SendMessageAsync($"```{_message}```");
                }
            }
        }
    }
}
