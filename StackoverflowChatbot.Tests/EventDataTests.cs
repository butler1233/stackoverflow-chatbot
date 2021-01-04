using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using StackoverflowChatbot.Services;
using StackoverflowChatbot.Services.Repositories;

namespace StackoverflowChatbot.Tests
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class EventDataTests
	{
		#region [ OneTimeSetUp ]

		[OneTimeSetUp]
		public static void SetupTestClass() =>
			Config.Manager.Config().Triggers = new List<string>()
			{
				"S, ",
				"Sandy, "
			};

		// Message is "S, say hello"
		private static readonly JToken _serializedData = JToken.Parse(System.IO.File.ReadAllText("EventData.json"));

		public static IEnumerable<TestCase> CommandsToTestAgainst()
		{
			yield return new TestCase()
			{
				TestData = GetDataWithCommand("S, say hello"),
				Trigger = "S, ",
				CommandName = "say",
				Parameters = "hello"
			};
			yield return new TestCase()
			{
				TestData = GetDataWithCommand("Sandy, shutdown"),
				Trigger = "Sandy, ",
				CommandName = "shutdown",
				Parameters = null
			};
		}

		private static EventData GetDataWithCommand(string command)
		{
			var newData = _serializedData.DeepClone();
			newData["content"] = command;
			return EventData.FromJson(newData);
		}

		#endregion

		[Test, TestCaseSource(nameof(CommandsToTestAgainst))]
		public void FromJson_ShouldBuildCorrectObject(TestCase testCase)
		{
			testCase.TestData.Username.Should().Be("Squirrelkiller");
			testCase.TestData.RoomName.Should().Be("Sandbox");
		}

		[Test, TestCaseSource(nameof(CommandsToTestAgainst))]
		public void Command_ShouldReturnCommandWithoutTrigger(TestCase testCase)
		{
			var expectedCommand = testCase.CommandName;
			if (testCase.Parameters != null)
			{
				expectedCommand += $" {testCase.Parameters}";
			}
			testCase.TestData.Command.Should().Be(expectedCommand);
		}

		[Test, TestCaseSource(nameof(CommandsToTestAgainst))]
		public void CommandName_ShouldReturnCommandNameOnly(TestCase testCase) =>
			testCase.TestData.CommandName.Should().Be(testCase.CommandName);

		[Test, TestCaseSource(nameof(CommandsToTestAgainst))]
		public void CommandParameters_ShouldReturnParametersAfterCommandName(TestCase testCase) =>
			testCase.TestData.CommandParameters.Should().Be(testCase.Parameters);

		[Test]
		[Ignore("Firebase OAuth disabled for the meantime...")]
		public async Task Firebase_TestData()
		{
			var config = Config.Manager.Config();
			var repository = new FirebaseRepositoryService(config.FirebaseProjectId);
			await repository.Stupid();
		}
	}

	[ExcludeFromCodeCoverage]
	public class TestCase
	{
		internal EventData TestData = null!;
		internal string Trigger = null!;
		internal string CommandName = null!;
		internal string? Parameters;
	}
}
