using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackoverflowChatbot.Relay;

namespace StackoverflowChatbot.Tests
{
	[TestClass]
	public class StackTests
	{
		private const int RoomId = 7;
		private const string RoomName = "c";

		[TestMethod]
		public void MessageHtml()
		{
			var html = "Foo &#xA9; bar";
			var text = html.ProcessStackMessage(RoomId, RoomName);
			Assert.AreEqual(text, "Foo © bar");
		}

		[TestMethod]
		public void MessageMultiline()
		{
			var html = "<div class=\"full\">a <br> a</div>";
			var text = html.ProcessStackMessage(RoomId, RoomName);
			Assert.AreEqual(text, "a\r\na");
		}

		[TestMethod]
		public void OneBoxCode()
		{
			var html = "<pre class=\"full\">@{\r\n    string email = item.Email;\r\n    if(item.isAdmin) email = $\"&lt;strong&gt;{email}&lt;/strong&gt;\r\n}\r\n@email</pre>";
			var text = html.ProcessStackMessage(RoomId, RoomName);
			Assert.AreEqual(text, "```\r\n@{\r\n    string email = item.Email;\r\n    if(item.isAdmin) email = $\"<strong>{email}</strong>\r\n}\r\n@email\r\n```");
		}

		[TestMethod]
		public void OneboxMessage()
		{
			var html = "<div class=\"onebox ob-message\"><a rel=\"noopener noreferrer\" class=\"roomname\" href=\"/transcript/message/50954410#50954410\"><span title=\"2020-11-18 09:25:12Z\">46 mins ago</span></a>, by <span class=\"user-name\">d4rk4ng31</span> <br><div class=\"quote\">@Botler SHUT THE FUCK UP!</div></div>";
			var text = html.ProcessStackMessage(RoomId, RoomName);
			Assert.AreEqual(text, "https://chat.stackoverflow.com/transcript/message/50954410#50954410");
		}

		[TestMethod]
		public void OneboxImage()
		{
			var html = "<div class=\"onebox ob-image\"><a rel=\"nofollow noopener noreferrer\" href=\"https://i.redd.it/7apl34bga1061.png\"><img src=\"https://i.redd.it/7apl34bga1061.png\" class=\"user-image\" alt=\"user image\"></a></div>";
			var text = html.ProcessStackMessage(RoomId, RoomName);
			Assert.AreEqual(text, "https://i.redd.it/7apl34bga1061.png");
		}

		[TestMethod]
		public void OneboxXkcd()
		{
			var html = "<div class=\"onebox ob-xkcd\"><a rel=\"nofollow noopener noreferrer\" href=\"https://xkcd.com/2387\"><img src=\"https://imgs.xkcd.com/comics/blair_witch.png\" title=\"&quot;Are you concerned the witches won't breed in captivity?&quot; &quot;Honestly, we're more concerned that they WILL. We don't know what it involves, but our biologists theorize that it's 'harrowing.'&quot;\" alt=\"&quot;Are you concerned the witches won't breed in captivity?&quot; &quot;Honestly, we're more concerned that they WILL. We don't know what it involves, but our biologists theorize that it's 'harrowing.'&quot;\"></a></div>";
			var text = html.ProcessStackMessage(RoomId, RoomName);
			Assert.AreEqual(text, "https://imgs.xkcd.com/comics/blair_witch.png");
		}

		[TestMethod]
		public void OneboxYouTube()
		{
			var html = "<div class=\"onebox ob-youtube\"><a rel=\"nofollow noopener noreferrer\" style=\"text-decoration: none;\" href=\"https://www.youtube.com/watch?v=A_sY2rjxq6M\"><img src=\"https://i2.ytimg.com/vi/A_sY2rjxq6M/hqdefault.jpg\" class=\"ob-youtube-preview\" width=\"240\" height=\"180\"><div class=\"ob-youtube-title\">The Trammps - Disco Inferno</div><div class=\"ob-youtube-overlay\">►</div></a></div>";
			var text = html.ProcessStackMessage(RoomId, RoomName);
			Assert.AreEqual(text, "https://www.youtube.com/watch?v=A_sY2rjxq6M");
		}
	}
}