using Botler.Core.Config;
using Botler.Database.Dbos;
using Microsoft.EntityFrameworkCore;

namespace Botler.Database;

public class SqliteContext : DbContext
{
	public DbSet<MessageDbo> Messages { get; set; }

	public SqliteContext()
	{
		
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var config = Manager.Config();
		var log = Manager.Logger;

		log.Information("Configuring EF context...");

		if (config.UseMssql)
		{
			log.Information("Configuring MSSQL connection...");

			if (string.IsNullOrEmpty(config.MssqlConnectionString))
			{
				log.Error("MSSQL connection string is null or empty! Turn off `UseMssql` or supply a connection string.");
			}

			optionsBuilder.UseSqlServer(config.MssqlConnectionString,
				opt => opt.MigrationsAssembly("Botler.Migrations.Mssql"));
		}
		else
		{
			log.Warning("SQLite database is currently enabled! Consider using MSSQL instead - it's more stable and less crashy.");

			var dbPath = config.SqliteFilename;

			log.Information("Loading SQLite db file: {0}", dbPath);
			var dbinfo = new FileInfo(dbPath);
			if (dbinfo.Exists)
			{
				log.Information("Sqlite db exists! Size {0} bytes", dbinfo.Length);
			}
			else
			{
				throw new FileNotFoundException("DB file was not found", dbPath);
			}

			optionsBuilder.UseSqlite($"Data Source={dbPath}",
				opt => opt.MigrationsAssembly("Botler.Migrations.Sqlite"));
		}
		
		
		

		
	}
}