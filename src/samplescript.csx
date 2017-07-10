#! "netcoreapp1.1"
//#r "nuget:NetStandard.Library,1.6.1"
#r "nuget:Microsoft.EntityFrameworkCore.SqlServer,1.1.2"
// #r "nuget:System.Data.Common,4.3.0"
//#r "nuget:System.Data.SqlClient,4.1.0"

using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

// throw new Exception();
return;
public class TempContext : DbContext
{
    public DbSet<Log> Logs { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\bri\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\TestBase.mdf;Integrated Security=True");
}

public class Log
{
    public int Id { get; set; }
    public string Value { get; set; }
}
var context = new TempContext();
var logQuery = context.Logs.Where(l => l.Id == 1).ToList();
WriteLine(logQuery);
WriteLine("sdfsdf");