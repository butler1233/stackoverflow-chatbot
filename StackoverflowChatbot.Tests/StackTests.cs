using StackoverflowChatbot.Relay;
using FluentAssertions;
using NUnit.Framework;

namespace StackoverflowChatbot.Tests
{
	[TestFixture]
	public class StackTests
	{
		private const int RoomId = 7;
		private const string RoomName = "c";

		[TestCase("Hello <b>world</b>, how are <i>you?</i>", "Hello world, how are you?", TestName = "Message with multiple blocks")]
		[TestCase("Foo &#xA9; bar", "Foo © bar", TestName = "Message html")]
		[TestCase("<div class=\"full\">a <br> a</div>", "a \r\na", TestName = "Message multiline")]
		[TestCase("<pre class=\"full\">@{\r\n    string email = item.Email;\r\n    if(item.isAdmin) email = $\"&lt;strong&gt;{email}&lt;/strong&gt;\r\n}\r\n@email</pre>", "```\r\n@{\r\n    string email = item.Email;\r\n    if(item.isAdmin) email = $\"<strong>{email}</strong>\r\n}\r\n@email\r\n```", TestName = "Onebox code")]
		[TestCase("<div class=\"onebox ob-message\"><a rel=\"noopener noreferrer\" class=\"roomname\" href=\"/transcript/message/50954410#50954410\"><span title=\"2020-11-18 09:25:12Z\">46 mins ago</span></a>, by <span class=\"user-name\">d4rk4ng31</span> <br><div class=\"quote\">@Botler SHUT THE FUCK UP!</div></div>", "https://chat.stackoverflow.com/transcript/message/50954410#50954410", TestName = "Onebox message")]
		[TestCase("<div class=\"onebox ob-image\"><a rel=\"nofollow noopener noreferrer\" href=\"https://i.redd.it/7apl34bga1061.png\"><img src=\"https://i.redd.it/7apl34bga1061.png\" class=\"user-image\" alt=\"user image\"></a></div>", "https://i.redd.it/7apl34bga1061.png", TestName = "Onebox image")]
		[TestCase("<div class=\"onebox ob-xkcd\"><a rel=\"nofollow noopener noreferrer\" href=\"https://xkcd.com/2387\"><img src=\"https://imgs.xkcd.com/comics/blair_witch.png\" title=\"&quot;Are you concerned the witches won't breed in captivity?&quot; &quot;Honestly, we're more concerned that they WILL. We don't know what it involves, but our biologists theorize that it's 'harrowing.'&quot;\" alt=\"&quot;Are you concerned the witches won't breed in captivity?&quot; &quot;Honestly, we're more concerned that they WILL. We don't know what it involves, but our biologists theorize that it's 'harrowing.'&quot;\"></a></div>", "https://imgs.xkcd.com/comics/blair_witch.png", TestName = "Onebox XKCD")]
		[TestCase("<div class=\"onebox ob-youtube\"><a rel=\"nofollow noopener noreferrer\" style=\"text-decoration: none;\" href=\"https://www.youtube.com/watch?v=m3_I2rfApYk\"><img src=\"https://i2.ytimg.com/vi/m3_I2rfApYk/hqdefault.jpg\" width=\"240\" height=\"180\" class=\"ob-youtube-preview\"/><div class=\"ob-youtube-title\">Brexit, Briefly</div><div class=\"ob-youtube-overlay\">►</div></a></div>\r\n[Captain Obvious] botler, shutdown", "https://www.youtube.com/watch?v=m3_I2rfApYk", TestName = "Onebox youtube")]
		public void TestRawInputs(string html, string expected) => html.ProcessStackMessage(RoomId, RoomName).Should().Be(expected);
	}
}