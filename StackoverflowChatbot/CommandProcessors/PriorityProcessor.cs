using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Extensions;
using StackoverflowChatbot.Helpers;
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
		private readonly ICommandStore commandStore;
		private readonly IHttpService httpService;
		private readonly int roomId;
		private readonly IReadOnlyDictionary<string, Type> nativeCommands;

		public PriorityProcessor(
			IRoomService roomService,
			ICommandStore commandService,
			IHttpService httpService,
			int roomId)
		{
			this.roomService = roomService;
			this.commandStore = commandService;
			this.httpService = httpService;
			this.roomId = roomId;
			this.nativeCommands = this.LoadNativeCommands().ToDictionary(kvp => kvp.commandName, kvp => kvp.implementer);
		}

		private IEnumerable<(string commandName, Type implementer)> LoadNativeCommands()
		{
			var implementers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly =>
				assembly.GetTypes().Where(x => typeof(BaseCommand).IsAssignableFrom(x) && !x.IsAbstract));

			foreach (var implementer in implementers)
			{
				var instance = this.CreateCommandInstance(implementer);
				if (instance != null)
				{
					Console.WriteLine(
						$"Loaded command {instance.CommandName()} from type {implementer.Name} from {implementer.Assembly.FullName}");
					yield return (instance.CommandName().ToLower(), implementer);
				}
				else
				{
					Console.WriteLine($"Could not load command {implementer.FullName}.");
				}
			}
		}

		// Key: Room; Value: Set of users who summoned to this room;
		private readonly Dictionary<int, HashSet<int>> peopleWhoSummoned = new Dictionary<int, HashSet<int>>();

		/// <summary>
		/// Process the event if a suitable command is found.
		/// </summary>
		/// <returns>Whether or not the event was processed.</returns>
		public bool ProcessCommand(EventData data, out IAction? action)
		{
			var command = data.Command;

			if (this.TryGetNativeCommand(data, out action))
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

		public bool TryGetNativeCommands(string key, out Type? value) => this.nativeCommands.TryGetValue(key, out value);
		public IEnumerable<string> NativeKeys => this.nativeCommands.Keys;

		//NativeKeys, 
		private bool TryGetNativeCommand(EventData data, out IAction? action)
		{
			if (this.nativeCommands.TryGetValue(data.CommandName, out var commandType))
			{
				var instance = this.CreateCommandInstance(commandType);
				action = instance?.ProcessMessage(data,
					data.CommandParameters?.Split(" "));
				return action != null;
			}

			action = null;
			return false;
		}

		private BaseCommand CreateCommandInstance(Type commandType)
		{
			var parameterTypes = commandType
				.GetConstructors()
				.First()
				.GetParameters()
				.Select(e => e.ParameterType);

			var parameterValues = new List<object>();
			foreach(var param in parameterTypes)
			{
				if (param == typeof(ICommandStore))
					parameterValues.Add(this.commandStore);
				else if (param == typeof(ICommandProcessor))
					parameterValues.Add(this);
			}

			if (parameterValues.Any())
				return (BaseCommand)Activator.CreateInstance(commandType, parameterValues.ToArray())!;

			return (BaseCommand)Activator.CreateInstance(commandType)!;
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
			var command = new CustomCommand(name, args);
			if (DynamicCommand.TryParse(args, out var dynamicCommand))
			{
				command.IsDynamic = true;
				command.ExpectedDynamicCommandArgs = dynamicCommand!.ExpectedArgsCount;
			}

			_ = this.commandStore.AddCommand(command)
				.ContinueWith(async t =>
				{
					if (t.IsFaulted)
					{
						Exception? exception = t.Exception;
						while (exception is AggregateException aggregateException)
							exception = aggregateException.InnerException;
						Console.Write(exception);
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

			if (!this.peopleWhoSummoned.ContainsKey(room))
			{
				this.peopleWhoSummoned.Add(room, new HashSet<int>());
			}

			if (data.SentByController())
			{
				var joinedByAdmin = this.roomService.JoinRoom(room);
				return NewMessageAction(joinedByAdmin ? $"I joined room {room}, Boss." : $"Couldn't join room {room}, guess I'm already there!");
			}

			if (this.peopleWhoSummoned[room].Count < NumberOfRequiredSummons)
			{
				_ = this.peopleWhoSummoned[room].Add(data.UserId);
				return NewMessageAction($"{NumberOfRequiredSummons - this.peopleWhoSummoned[room].Count} more and I'll join room {room}");
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

		private Task<HashSet<CustomCommand>> GetCustomCommands() => this.commandStore.GetCommands();

		public async Task<IAction?> ProcessCommandAsync(EventData data)
		{
			var commandList = await this.GetCustomCommands();
			var command = commandList.FirstOrDefault(e => e.Name == data.CommandName);
			if (command != null)
			{
				if (command.IsDynamic)
				{
					return await this.ProcessDynamicCommand(data, command);
				}
				return NewMessageAction(command!.Parameter!);
			}
			return null;
		}

		private async Task<IAction?> ProcessDynamicCommand(EventData data, CustomCommand command)
		{
			if (!DynamicCommand.TryParse(command.Parameter!, out var dynaCmd))
			{
				return NewMessageAction("Corrupted dynamic command");
			}

			var param = HttpUtility.HtmlDecode(data.CommandParameters ?? "");
			var args = param?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			args = CliHelper.CombineOption(args, ' ').ToArray();
			var expectedLength = command.ExpectedDynamicCommandArgs;

			if (args == null || args.Length != expectedLength)
				return NewMessageAction($"Expected {expectedLength} args, found {args?.Length ?? 0} for command '{command.Name}'");

			var timeout = TimeSpan.FromSeconds(10);
			try
			{
				var api = Uri.UnescapeDataString(dynaCmd!.ApiAddress.AbsoluteUri);
				api = HttpUtility.HtmlDecode(string.Format(api, args));
				if (dynaCmd.Method == Method.Get && dynaCmd.ResponseType == ResponseType.Image)
				{
					var alias = dynaCmd.Alias;
					// TODO Discord doesn't have a hyperlink markdown yet unfortunately.
					// but we can use this: https://leovoel.github.io/embed-visualizer/
					api = string.IsNullOrEmpty(alias) ? api : $"[{alias}]({api})";
					return NewMessageAction(api);
				}
				var cts = new CancellationTokenSource(timeout);
				var apiResponse = await this.Fetch(api, dynaCmd.Method, dynaCmd.ContentType, cts.Token);
				var stringContent = apiResponse.ToString() ?? "{}";
				if (string.IsNullOrEmpty(dynaCmd.JsonPath))
					return NewMessageAction(stringContent!);
				var obj = JObject.Parse(stringContent);
				var response = obj.SelectToken(dynaCmd.JsonPath)?.ToString() ?? stringContent;
				return NewMessageAction(response);
			}
			catch (OperationCanceledException)
			{
				return NewMessageAction($"I can't wait for longer than {timeout:s} and the API is slow.");
			}
		}

		private Task<object> Fetch(string api, Method method, ContentType contentType, CancellationToken cancellationToken)
		{
			switch (method)
			{
				case Method.Post:
					var components = api.Split('?', 2, StringSplitOptions.RemoveEmptyEntries);
					var domain = components.First();
					switch (contentType)
					{
						case ContentType.Json:
						default:
							var d1 = components.Length > 1 ? QueryStringHelper.ToObject<object>(components.Last()) : null;
							return this.httpService.PostJson<object>(new Uri(domain!), d1?.ToString(), cancellationToken!);
						case ContentType.Form:
							var d2 = components.Length > 1 ? QueryStringHelper.ToDictionary(components.Last()) : new Dictionary<string, string>();
							return this.httpService.PostUrlEncoded<object>(new Uri(domain!), d2!, cancellationToken!);
						case ContentType.Multi:
							// TODO this will crash
							var d3 = components.Length > 1 ? components.Last().AsDictionary() : new Dictionary<string, object?>();
							return this.httpService.PostMultipart<object>(new Uri(domain!), d3!, cancellationToken!);
					}
				case Method.Get:
				default:
					return this.httpService.Get<object>(new Uri(api!), cancellationToken!);
			}
		}
	}
}
