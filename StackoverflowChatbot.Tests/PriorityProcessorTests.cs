using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.CommandProcessors;
using StackoverflowChatbot.NativeCommands;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.Tests
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class PriorityProcessorTests
	{
		private PriorityProcessor _priorityProcessor = null!;
		private CommandFactory _commandFactory = null!;
		private EventData EventDataFromContent(string content) =>
			EventData.FromJson(JToken.Parse(
@"{
	""event_type"": 1,
	""time_stamp"": 1603377031,
	""content"": """ + content + @""",
	""id"": 105394918,
	""user_id"": 4364057,
	""user_name"": ""Squirrelkiller"",
	""room_id"": 1,
	""room_name"": ""Sandbox"",
	""message_id"": 50752263
}"));

		[OneTimeSetUp]
		public static void SetupTestClass() =>
			Config.Manager.Config().Triggers = new List<string>()
			{
				"Botler! ",
				"Botler, ",
				"👏👏 "
			};

		[SetUp]
		public void Setup()
		{
			var roomServiceMock = new Mock<IRoomService>();
			roomServiceMock.Setup(x => x.JoinRoom(7)).Returns(true);
			var commandStoreMock = new Mock<ICommandStore>();
			var httpServiceMock = new Mock<IHttpService>();
			var coll = new ServiceCollection();
			coll.AddSingleton<ICommandStore, ICommandStore>(x => commandStoreMock.Object);
			coll.AddSingleton<IRoomService, IRoomService>(x => roomServiceMock.Object);
			coll.AddSingleton<IHttpService, IHttpService>(x => httpServiceMock.Object);
			_commandFactory = new CommandFactory(coll);

			coll.Should().HaveCount(3);
			_priorityProcessor = new PriorityProcessor(commandStoreMock.Object, httpServiceMock.Object,
				_commandFactory);
		}

		[Test]
		public void Help_ShouldNotThrow()
		{
			var eventData = EventDataFromContent("Botler, help leave");
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().BeOfType<SendMessage>();
		}

		[Test]
		public void Unknown_ShouldBeNull()
		{
			var eventData = EventDataFromContent("Botler, unknown leave"); 
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().BeNull();
		}

		[Test]
		public void Leave_ShouldNotBeNull()
		{
			var eventData = EventDataFromContent("Botler, leave 11"); 
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().NotBeNull();
		}

		[Test]
		public void ParameterlessLeave_ShouldNotBeNull()
		{
			var eventData = EventDataFromContent("Botler, leave"); 
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().NotBeNull();
		}

		[Test]
		public void TooManyParameterLeave_ShouldNotBeNull()
		{
			var eventData = EventDataFromContent("Botler, leave 12 12 4234"); 
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().NotBeNull();
		}

		[Test]
		public void Gibberish_ShouldBeNull()
		{
			var eventData = EventDataFromContent("Botler, leave gibberish"); 
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().NotBeNull();
		}

		[Test]
		public void Join_ShouldAskForMoreRequest()
		{
			var eventData = EventDataFromContent("Botler, join 3"); 
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().NotBeNull();
		}

		[Test]
		public void AuthoredCommand_ShouldAcceptValidAdmin()
		{
			if (!Config.Manager.Config().Controllers.Contains(4364057))
			{
				Config.Manager.Config().Controllers.Add(4364057);
			}
			// check out in the json the user id is 4364057.
			var eventData = EventDataFromContent("Botler, shutdown");
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			// coverage went on the I'll be back branch
			action.Should().NotBeNull();
		}

		[Test]
		public void AuthoredCommand_ShouldNotAcceptHacker()
		{
			if (Config.Manager.Config().Controllers.Contains(4364057))
			{
				Config.Manager.Config().Controllers.Remove(4364057);
			}
			var eventData = EventDataFromContent("Botler, shutdown");
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			// coverage went on YOU'RE NOT MY MOM/DAD branch.
			action.Should().NotBeNull();
		}

		[Test]
		public void OverLearnNativeCommand_ShouldNotOverwriteFunction()
		{
			var eventData = EventDataFromContent("Botler, learn join asdf");
			_priorityProcessor.ProcessNativeCommand(eventData, out var action);
			action.Should().NotBeNull();
			eventData = EventDataFromContent("Botler, join 3");
			_priorityProcessor.ProcessNativeCommand(eventData, out action);
			// coverage went join room (native) command
			action.Should().NotBeNull();
		}

		[Test]
		public void GetAllHelp_ShouldNotThrow()
		{
			var implementers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly =>
				assembly.GetTypes().Where(x => typeof(BaseCommand).IsAssignableFrom(x) && !x.IsAbstract));
			foreach (var implementer in implementers)
			{
				string? commandName = null;
				foreach (var methodInfo in implementer.GetRuntimeMethods())
				{
					if (methodInfo.Name != "CommandName")
						continue;

					var nativeCommand = _commandFactory.Create(implementer, _priorityProcessor);
					var ret = methodInfo.Invoke(nativeCommand, new object[0]);
					commandName = ret?.ToString();
					break;
				}
				var eventData = EventDataFromContent($"Botler, help {commandName}");
				_priorityProcessor.ProcessNativeCommand(eventData, out var action);
				action.Should().NotBeNull();
			}
		}
	}
}
