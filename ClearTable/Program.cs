using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClearTable
{
    class Program
    {
        static string connectionString = string.Empty;
        static string codesTableName = string.Empty;

        static void Main(string[] args)
        {
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json")));
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);
            codesTableName = jo["CodeSource"].ToString();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var cmdDelete = new SQLiteCommand($"{codesTableName}", conn);
                cmdDelete.ExecuteNonQuery();
            }

            Console.WriteLine($"OK!");
        }
    }
}
