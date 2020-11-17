using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace StackoverflowChatbot.Tests
{
	[TestClass]
	public class EventDataTests
	{
		#region [ Setup ]

		[ClassInitialize]
		public static void SetupTestClass(TestContext _) =>
			Config.Manager.Config().Triggers = new List<string>()
			{
				"S, ",
				"Sandy, "
			};

		// Message is "S, say hello"
		private static readonly JToken serializedData = JToken.Parse(System.IO.File.ReadAllText("EventData.json"));

		public static IEnumerable<TestCase[]> CommandsToTestAgainst()
		{
			yield return new[]{ new TestCase()
			{
				TestData = GetDataWithCommand("S, say hello"),
				Trigger = "S, ",
				CommandName = "say",
				Parameters = "hello"
			} };
			yield return new[]{ new TestCase()
			{
				TestData = GetDataWithCommand("Sandy, shutdown"),
				Trigger = "Sandy, ",
				CommandName = "shutdown",
				Parameters = null
			} };
		}

		private static EventData GetDataWithCommand(string command)
		{
			var newData = serializedData.DeepClone();
			newData["content"] = command;
			return EventData.FromJson(newData);
		}

		#endregion

		[DataTestMethod]
		[DynamicData(nameof(CommandsToTestAgainst), DynamicDataSourceType.Method)]
		public void FromJson_ShouldBuildCorrectObject(TestCase testCase)
		{
			Assert.AreEqual("Squirrelkiller", testCase.TestData.Username);
			Assert.AreEqual("Sandbox", testCase.TestData.RoomName);
		}

		[DataTestMethod]
		[DynamicData(nameof(CommandsToTestAgainst), DynamicDataSourceType.Method)]
		public void Command_ShouldReturnCommandWithoutTrigger(TestCase testCase)
		{
			var expectedCommand = testCase.CommandName;
			if (testCase.Parameters != null)
			{
				expectedCommand += $" {testCase.Parameters}";
			}
			Assert.AreEqual(expectedCommand, testCase.TestData.Command);
		}

		[DataTestMethod]
		[DynamicData(nameof(CommandsToTestAgainst), DynamicDataSourceType.Method)]
		public void CommandName_ShouldReturnCommandNameOnly(TestCase testCase) =>
			Assert.AreEqual(testCase.CommandName, testCase.TestData.CommandName);

		[DataTestMethod]
		[DynamicData(nameof(CommandsToTestAgainst), DynamicDataSourceType.Method)]
		public void CommandParameters_ShouldReturnParametersAfterCommandName(TestCase testCase) =>
			Assert.AreEqual(testCase.Parameters, testCase.TestData.CommandParameters);
	}

	public class TestCase
	{
		internal EventData TestData = null!;
		internal string Trigger = null!;
		internal string CommandName = null!;
		internal string? Parameters;
	}
}
