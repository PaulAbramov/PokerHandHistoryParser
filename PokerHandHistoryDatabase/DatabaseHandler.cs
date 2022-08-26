using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace PokerHandHistoryDatabase
{
    public class DatabaseHandler
    {
        private static Queue<SqliteCommand> commands = new Queue<SqliteCommand>();
        private static SqliteConnection SQLiteConnection;


        public static void InitializeDatabase()
        {
            SQLiteConnection = new SqliteConnection(@"Data Source=PokerHandHistory.db;");

            var queryCreateAssetTable =
                "CREATE TABLE IF NOT EXISTS Hands" +
                "(" +
                "Id INTEGER PRIMARY KEY," +
                "Hand varchar(MAX) NOT NULL);";

            using (var cmd = new SqliteCommand(queryCreateAssetTable))
            {
                try
                {
                    SQLiteConnection.Open();
                    cmd.Connection = SQLiteConnection;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    SQLiteConnection.Close();
                    Console.WriteLine(e.Message);
                    throw;
                }

                SQLiteConnection.Close();
            }
        }

        public static void ExecuteCommands()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        var command = commands.Dequeue();

                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            SQLiteConnection.Close();

                            Console.WriteLine(e);
                            throw;
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                {
                    return;
                }
                Console.WriteLine(e);
                throw;
            }
        }

        public static void AddCommandToQueue(long _id, string _wholeString)
        {
            try
            {
                using (var cmd = new SqliteCommand($"INSERT INTO Hands (Id, Hand) VALUES (@Id, @Hand)", SQLiteConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", _id);
                    cmd.Parameters.AddWithValue("@Hand", _wholeString);

                    commands.Enqueue(cmd);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
