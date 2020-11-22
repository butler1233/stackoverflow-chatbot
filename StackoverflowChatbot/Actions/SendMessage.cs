using System.Threading.Tasks;
using Discord.WebSocket;
using SharpExchange.Chat.Actions;

namespace StackoverflowChatbot.Actions
{
    internal class SendMessage : IAction
    {
        private readonly string message;

        public SendMessage(string message) => this.message = message;
        public async Task Execute(ActionScheduler scheduler)
        {
            // Send message to chat.so
            await scheduler.CreateMessageAsync(this.message);

            // Send message to Discord
            var config = Config.Manager.Config();
            var channelName = config.StackToDiscordMap[scheduler.RoomId];
            var discordClient = await Discord.GetDiscord();
            var discord = discordClient.GetChannel(config.DiscordChannelNamesToIds[channelName]);

            if (discord is SocketTextChannel textChannel)
            {
				await textChannel.SendMessageAsync($"```{message}```");
            }
        }
    }
}
