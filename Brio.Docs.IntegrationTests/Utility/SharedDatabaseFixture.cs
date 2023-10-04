using System;
using System.Data.Common;
using Brio.Docs.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Tests.Utility
{
    public class SharedDatabaseFixture : IDisposable
    {
        private static readonly object SYNC = new object();
        private static bool _databaseInitialized;

        public SharedDatabaseFixture(Action<DMContext> seedAction = null)
        {
            Context = Seed(seedAction);
        }

        public DMContext Context { get; }

        private DbConnection Connection { get; set; }

        public DMContext CreateContext(DbTransaction transaction = null)
        {
            var options = new DbContextOptionsBuilder<DMContext>().UseSqlite(CreateInMemoryDatabase()).Options;
            var context = new DMContext(options);

            if (transaction != null)
                context.Database.UseTransaction(transaction);

            return context;
        }

        public void Dispose()
        {
            if (Connection != null)
                Connection.Dispose();

            if (Context != null)
                Context.Dispose();
        }

        private DMContext Seed(Action<DMContext> seed)
        {
            lock (SYNC)
            {
                //// FIXME: this check is needed when using shared connection to single database
                //// but SQLite in-memory DB is recreated every time when new connection is set
                //// so this check would prevent database to be initialized correctly
                //if (!_databaseInitialized)
                //{
                    DMContext context = CreateContext();
                    //context.Database.EnsureDeleted();
                    //context.Database.EnsureCreated();

                    // add initial database data here
                    seed?.Invoke(context);
                    //_databaseInitialized = true;

                    return context;
                //}
            }
            //return null;
        }

        private DbConnection CreateInMemoryDatabase()
        {
            Connection = new SqliteConnection("DataSource = file::memory:");
            Connection.Open();

            return Connection;
        }
    }
}
