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
				DbPath = Manager.Config().SqliteFilename;
			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			Console.WriteLine($"Configuring EF context - loading '{DbPath}'");
			var dbinfo = new FileInfo(DbPath);
			if (dbinfo.Exists)
			{
				Console.WriteLine($"Sqlite db exists! Size {dbinfo.Length} bytes");
			}
			else
			{
				throw new FileNotFoundException("DB file was not found", DbPath);
			}
			optionsBuilder.UseSqlite($"Data Source={DbPath}");
		}
	}
}
