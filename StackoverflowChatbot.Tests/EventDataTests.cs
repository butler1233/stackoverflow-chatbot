using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace StackoverflowChatbot.Tests
{
	[TestClass]
	public class EventDataTests
	{
		// Message is "S, say hello"
		private readonly JToken serializedData = JToken.Parse(System.IO.File.ReadAllText("EventData.json"));
		private EventData eventData;

		public IEnumerable<string> CommandsToTestAgainst() =>
			new[]
			{
				"S, say hello",
				"S, shutdown"
			};

		[TestInitialize]
		public void Setup() => this.eventData = EventData.FromJson(this.serializedData);

		[TestMethod]
		public void FromJson_ShouldBuildCorrectObject()
		{
			Assert.AreEqual("Squirrelkiller", this.eventData.Username);
			Assert.AreEqual("Sandbox", this.eventData.RoomName);
		}

		[TestMethod]
		public void Command_ShouldReturnCommandWithoutTrigger() => Assert.AreEqual("say hello", this.eventData.Command);

		[TestMethod]
		public void CommandName_ShouldReturnCommandNameOnly() => Assert.AreEqual("say", this.eventData.CommandName);

		[TestMethod]
		public void CommandParameters_ShouldReturnParametersAfterCommandName() =>
			Assert.AreEqual("hello", this.eventData.CommandParameters);
	}
}
