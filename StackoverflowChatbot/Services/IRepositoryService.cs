using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace StackoverflowChatbot.Services
{
	public interface IRepositoryService
	{
		Task<FirestoreDb> Database();
	}
}
