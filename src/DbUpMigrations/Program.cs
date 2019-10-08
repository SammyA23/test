using System;
using System.Linq;
using System.Reflection;
using DbUp;
using MfgConnection;

namespace DbUpMigrations
{
    class Program
    {
        static int Main(string[] args)
        {
              var mfgDbName = "MFG_EDI";
              var conn = new jbConnection();
              var dbServerLocation = conn.GetJbSettingsServer();

              var connectionStringToMfgDb = "Data Source=" + dbServerLocation
              + ";Initial Catalog=" + mfgDbName
              + ";User ID=jobboss;Password=Bali;User Instance=false;Trusted_connection=true;MultipleActiveResultSets=false";

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionStringToMfgDb)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}
