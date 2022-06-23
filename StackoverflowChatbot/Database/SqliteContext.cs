using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StackoverflowChatbot.Config;
using StackoverflowChatbot.Database.Dbos;

namespace StackoverflowChatbot.Database
{
	public class SqliteContext : DbContext
	{
		public DbSet<MessageDbo> Messages { get; set; }

		public static string? DbPath { get; private set; } = null;

		public SqliteContext()
		{
			if (DbPath == null)
			{
				string appDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
				Base config = Manager.Config();
				DbPath = Path.Combine(appDirectory, config.SqliteFilename);
			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source={DbPath}");
	}
}
