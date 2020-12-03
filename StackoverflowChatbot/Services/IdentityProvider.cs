using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.Services
{
	public class IdentityProvider: IIdentityProvider
	{
		public IdentityProvider(string username, string password)
		{
			Username = username;
			Password = password;
		}

		public string Username { get; }

		public string Password { get; }
	}
}
