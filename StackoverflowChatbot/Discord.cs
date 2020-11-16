using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SharpExchange.Chat.Actions;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;

namespace StackoverflowChatbot
{
	internal static class Discord
	{

		private static DiscordSocketClient _client = null;

		internal static DiscordSocketClient GetDiscord()
		{
			if (_client == null)
			{
				InitialiseDiscord();
			}

			return _client;
		}

		internal static Dictionary<int,RoomWatcher<DefaultWebSocket>> StackRoomWatchers = new Dictionary<int, RoomWatcher<DefaultWebSocket>>();
		internal static Dictionary<int,ActionScheduler> StackSchedulers = new Dictionary<int, ActionScheduler>();

		private static void InitialiseDiscord()
		{
			var client = new DiscordSocketClient();
			//Setuo handlers
			client.MessageReceived += ClientRecieved;
			//Logs in
			client.LoginAsync(TokenType.Bot, Config.Manager.Config().DiscordToken);
			client.StartAsync();
			while (client.LoginState != LoginState.LoggedIn)
			{
				Thread.Sleep(100);
			}
			//Now wr're done
			_client = client;
		}

		private static Task ClientRecieved(SocketMessage arg)

		{

			if (arg.Author is SocketGuildUser user)
			{
				if (arg.Author.IsBot) return Task.CompletedTask;
				var config = Config.Manager.Config();
				Console.WriteLine($"[DIS {arg.Channel.Name}] {arg.Content}");
				//Check if we have a mapping.
				if (config.DiscordToStackMap.ContainsKey(arg.Channel.Name))
				{
					//We are setup to map this channel's messages to stack.
					var roomId = config.DiscordToStackMap[arg.Channel.Name];
					//Build the message
					var message = $@"\[**{user.Nickname}** *(on [Discord]({config.DiscordInviteLink}))*] {arg.Content}";
					//Find the room scheduler
					if (StackSchedulers.ContainsKey(roomId))
					{
						//We already have a scheduler, lets goooo
						var sched = StackSchedulers[roomId];
						sched.CreateMessageAsync(message);
						return Task.CompletedTask;
					}
					//Or create one if we already have a watcher.
					if (StackRoomWatchers.ContainsKey(roomId))
					{
						var watcher = StackRoomWatchers[roomId];
						var newScheduler = new ActionScheduler(watcher.Auth, RoomService.Host, roomId);
						StackSchedulers.Add(roomId, newScheduler);
						newScheduler.CreateMessageAsync(message);
						arg.Channel.SendMessageAsync("Opened a new scheduler for sending messages to Stack. FYI.");
						return Task.CompletedTask;
					}
					//or complain about not watching stack.
					arg.Channel.SendMessageAsync(
						"Unable to sync messages to Stack - I'm not watching the corresponding channel. Invite me to the channel on stack and tryagain.");
					return Task.CompletedTask;
				}
				//Nothing to do, who cares
				return Task.CompletedTask;
			}

			return Task.CompletedTask;
		}


	}
}
