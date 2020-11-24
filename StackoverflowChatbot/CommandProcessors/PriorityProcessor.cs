using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.NativeCommands;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.CommandProcessors
{
	/// <summary>
	/// Processes commands that trigger basic functionality that always happens before any other processors.
	/// </summary>
	internal class PriorityProcessor: ICommandProcessor
	{
		private const int NumberOfRequiredSummons = 3;

		private readonly IRoomService roomService;
		private readonly ICommandService commandService;
		private readonly int roomId;

		internal static readonly IReadOnlyDictionary<string, Type> NativeCommands =
			LoadNativeCommands().ToDictionary(kvp => kvp.commandName, kvp => kvp.implementer);

		private IList<CustomCommand>? commandList = new List<CustomCommand>();

		private static IEnumerable<(string commandName, Type implementer)> LoadNativeCommands()
		{
			var implementers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly =>
				assembly.GetTypes().Where(x => typeof(BaseCommand).IsAssignableFrom(x) && !x.IsAbstract));

			foreach (var implementer in implementers)
			{
				var instance = (BaseCommand)Activator.CreateInstance(implementer)!;
				Console.WriteLine(
					$"Loaded command {instance.CommandName()} from type {implementer.Name} from {implementer.Assembly.FullName}");
				yield return (instance.CommandName().ToLower(), implementer);
			}
		}

		// Key: Room; Value: Set of users who summoned to this room;
		private static readonly Dictionary<int, HashSet<int>> peopleWhoSummoned = new Dictionary<int, HashSet<int>>();

		public PriorityProcessor(IRoomService roomService, ICommandService commandService, int roomId)
		{
			this.roomService = roomService;
			this.commandService = commandService;
			this.roomId = roomId;
		}

		/// <summary>
		/// Process the event if a suitable command is found.
		/// </summary>
		/// <returns>Whether or not the event was processed.</returns>
		public bool ProcessCommand(EventData data, out IAction? action)
		{
			var command = data.Command;

			if (TryGetNativeCommand(data, out action))
			{
				return true;
			}

			if (IsCommand(command, "leave", out var commandParameter))
			{
				action = this.LeaveRoomCommand(commandParameter);
				return true;
			}

			if (IsCommand(command, "join", out _))
			{
				action = this.JoinRoomCommand(data);
				return true;
			}

			if (IsCommand(command, "learn", out commandParameter))
			{
				action = this.LearnCommand(commandParameter);
				return true;
			}

			// Why is action getting assigned but not unused?
			action = null;
			return false;
		}

		private static bool TryGetNativeCommand(EventData data, out IAction? action)
		{
			if (NativeCommands.TryGetValue(data.CommandName, out var commandType))
			{
				action = ((BaseCommand)Activator.CreateInstance(commandType)!)?.ProcessMessage(data,
					data.CommandParameters?.Split(" "));
				return action != null;
			}

			action = null;
			return false;
		}

		private static bool IsCommand(string commandMessage, string commandKeyword, out string commandParameter)
		{
			var match = Regex.Match(commandMessage, $"^{commandKeyword} (.+)");

			if (match.Success)
			{
				commandParameter = match.Groups[1].Value;
				return true;
			}

			commandParameter = string.Empty;
			return false;
		}

		private IAction LeaveRoomCommand(string commandParameter)
		{
			// Can be told to leave other rooms.
			this.roomService.LeaveRoom(IsSingleNumber(commandParameter.Trim(), out var room) ? room : this.roomId);
			return NewMessageAction(room > 0 ? $"Leaving room {room}!" : "Bye!");
		}

		// TODO wrap to Task
		private IAction LearnCommand(string commandParameter)
		{
			var @params = commandParameter.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
			if (@params.Length < 2)
			{
				return NewMessageAction("Missing args");
			}

			var name = @params[0];
			var args = @params[1];
			_ = this.commandService.AddCommand(name, args)
				.ContinueWith(async t =>
				{
					if (t.IsFaulted)
					{
						Exception? exception = t.Exception;
						while (exception is AggregateException aggregateException)
							exception = aggregateException.InnerException;
						Console.Write(exception);
					}
					else
					{
						await this.CacheCommand(new CustomCommand(name, args));
					}
				});
			return NewMessageAction($"Learned the command {name}");
		}

		private SendMessage JoinRoomCommand(EventData data)
		{
			if (!int.TryParse(data.Command.Replace("join ", ""), out var room))
			{
				return NewMessageAction($"Couldn't find a valid room number.");
			}

			if (!peopleWhoSummoned.ContainsKey(room))
			{
				peopleWhoSummoned.Add(room, new HashSet<int>());
			}

			if (data.SentByController())
			{
				var joinedByAdmin = this.roomService.JoinRoom(room);
				return NewMessageAction(joinedByAdmin ? $"I joined room {room}, Boss." : $"Couldn't join room {room}, guess I'm already there!");
			}

			if (peopleWhoSummoned[room].Count < NumberOfRequiredSummons)
			{
				_ = peopleWhoSummoned[room].Add(data.UserId);
				return NewMessageAction($"{NumberOfRequiredSummons - peopleWhoSummoned[room].Count} more and I'll join room {room}");
			}

			var joined = this.roomService.JoinRoom(room);
			return NewMessageAction(joined ? $"I joined room {room}." : $"Couldn't join room {room}, guess I'm already there!");
		}

		private static SendMessage NewMessageAction(string message) => new SendMessage(message);

		private static bool IsSingleNumber(string v, out int room)
		{
			if (int.TryParse(v, out var number))
			{
				room = number;
				return true;
			}

			room = -1;
			return false;
		}

		private async Task EnsureCommandListReady()
		{
			if (this.commandList == null || this.commandList.Any())
			{
				this.commandList = await this.commandService.GetCommands();
			}
		}

		private async Task CacheCommand(CustomCommand command)
		{
			await this.EnsureCommandListReady();
			this.commandList?.Add(command);
		}

		public async Task<IAction?> ProcessCommandAsync(EventData data)
		{
			await this.EnsureCommandListReady();
			var result = this.commandList?.FirstOrDefault(e => e.Name == data.CommandName);
			if (result != null)
			{
				return NewMessageAction(result.Parameter!);
			}

			return null;
		}
	}
}
