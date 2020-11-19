using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.Services
{
	public class IdentityProvider: IIdentityProvider
	{
		public IdentityProvider(string username, string password)
		{
			this.Username = username;
			this.Password = password;
		}

		public string Username { get; private set; }

		public string Password { get; private set; }
	}
}
