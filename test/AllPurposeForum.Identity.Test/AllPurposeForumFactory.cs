using System.Data.Common;
using AllPurposeForum.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace AllPurposeForum.Identity.Test;

public class AllPurposeForumFactory : IDisposable
{
    public AllPurposeForumFactory()
    {
        // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
        // at the end of the test (see Dispose below).
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        // These options will be used by the context instances in this test suite, including the connection opened above.
        _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the schema and seed some data
        Context = new ApplicationDbContext(_contextOptions);
        Context.Database.EnsureCreated();
        Context.SaveChanges();
        
    }

    public ApplicationDbContext Context { get; private set; }
    private readonly DbConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

    public void Dispose()
    {
        Context?.Dispose();
        _connection?.Dispose();
    }
}