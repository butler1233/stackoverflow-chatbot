using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.Services
{
	public interface IIdentityProvider
	{
		public string Username { get; }
		public string Password { get; }
	}
}
