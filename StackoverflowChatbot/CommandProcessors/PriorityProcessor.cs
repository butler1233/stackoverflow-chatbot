using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly TimeSpan _dynamicCommandTimeout = TimeSpan.FromSeconds(10);

		private readonly ICommandStore _commandStore;
		private readonly IHttpService _httpService;
		private readonly ICommandFactory _commandFactory;

		private readonly IReadOnlyDictionary<string, Type> _nativeCommands;

		public PriorityProcessor(
			ICommandStore commandService,
			IHttpService httpService,
			ICommandFactory commandFactory)
		{
			_commandStore = commandService;
			_httpService = httpService;
			_commandFactory = commandFactory;
			_nativeCommands = LoadNativeCommands();
		}

		private Dictionary<string, Type> LoadNativeCommands()
		{
			var commandNameImplementerTypeMapping = new Dictionary<string, Type>();
			var implementers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly =>
				assembly.GetTypes().Where(x => typeof(BaseCommand).IsAssignableFrom(x) && !x.IsAbstract));

			foreach (var implementer in implementers)
			{
				var instance = CreateCommandInstance(implementer);
				if (instance != null)
				{
					Console.WriteLine(
						$"Loaded command {instance.CommandName()} from type {implementer.Name} from {implementer.Assembly.FullName}");
					commandNameImplementerTypeMapping[instance.CommandName().ToLower()] = implementer;
				}
				else
				{
					Console.WriteLine($"Could not load command {implementer.FullName}.");
				}
			}

			return commandNameImplementerTypeMapping;
		}

		/// <summary>
		/// Process the event if a suitable command is found.
		/// </summary>
		/// <returns>Whether or not the event was processed.</returns>
		public bool ProcessNativeCommand(EventData data, out IAction? action)
		{
			if (_nativeCommands.TryGetValue(data.CommandName, out var commandType))
			{
				var instance = CreateCommandInstance(commandType);
				action = instance?.ProcessMessage(data,
					data.CommandParameters?.Split(" "));
				return action != null;
			}

			// Why is action getting assigned but is unused?
			action = null;
			return false;
		}

		public bool TryGetNativeCommands(string key, out Type? value) => _nativeCommands.TryGetValue(key, out value);
		public IEnumerable<string> NativeKeys => _nativeCommands.Keys;

		private BaseCommand CreateCommandInstance(Type commandType) => _commandFactory.Create(commandType, this);

		private static SendMessage NewMessageAction(string message) => new SendMessage(message);

		private Task<HashSet<CustomCommand>> GetCustomCommands() => _commandStore.GetCommands();

		public async Task<IAction?> ProcessDynamicCommandAsync(EventData data)
		{
			var commandList = await GetCustomCommands();
			var command = commandList.FirstOrDefault(e => e.Name == data.CommandName);
			if (command != null)
			{
				if (command.IsDynamic)
				{
					return await ProcessDynamicCommand(data, command);
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

			if (args.Length != expectedLength)
				return NewMessageAction($"Expected {expectedLength} args, found {args.Length} for command '{command.Name}'");

			try
			{
				var api = Uri.UnescapeDataString(dynaCmd!.ApiAddress.AbsoluteUri);
				args = args.Select(HttpUtility.UrlEncode).ToArray();
				api = HttpUtility.HtmlDecode(string.Format(api, args));
				if (dynaCmd.Method == Method.Get && dynaCmd.ResponseType == ResponseType.Image)
				{
					var alias = dynaCmd.Alias?.Trim();
					// TODO Discord doesn't have a hyperlink markdown yet unfortunately.
					// but we can use this: https://leovoel.github.io/embed-visualizer/
					api = string.IsNullOrEmpty(alias) ? api : $"[{alias}]({api})";
					return NewMessageAction(api);
				}
				var cts = new CancellationTokenSource(_dynamicCommandTimeout);
				var apiResponse = await Fetch(api, dynaCmd.Method, dynaCmd.ContentType, cts.Token);
				var stringContent = apiResponse.ToString() ?? "{}";
				if (string.IsNullOrEmpty(dynaCmd.JsonPath))
					return NewMessageAction(stringContent!);
				var obj = JObject.Parse(stringContent);
				var response = obj.SelectToken(dynaCmd.JsonPath)?.ToString() ?? stringContent;
				return NewMessageAction(response);
			}
			catch (OperationCanceledException)
			{
				return NewMessageAction($"I can't wait for longer than {_dynamicCommandTimeout:s} and the API is slow.");
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
							return _httpService.PostJson<object>(new Uri(domain!), d1?.ToString(), cancellationToken!);
						case ContentType.Form:
							var d2 = components.Length > 1 ? QueryStringHelper.ToDictionary(components.Last()) : new Dictionary<string, string>();
							return _httpService.PostUrlEncoded<object>(new Uri(domain!), d2!, cancellationToken!);
						case ContentType.Multi:
							// TODO this will crash
							var d3 = components.Length > 1 ? components.Last().AsDictionary() : new Dictionary<string, object?>();
							return _httpService.PostMultipart<object>(new Uri(domain!), d3!, cancellationToken!);
					}
				case Method.Get:
				default:
					return _httpService.Get<object>(new Uri(api!), cancellationToken!);
			}
		}
	}
}
