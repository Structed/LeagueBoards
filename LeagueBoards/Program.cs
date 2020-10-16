using System;
using System.Data.SqlClient;
using System.Linq;

namespace LeagueBoards
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            { 
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.ConnectionString="";    // TODO: Add your connection string!

                var random = new Random();

                Console.WriteLine("Press A to add new records, press S to query scores!");
                ConsoleKeyInfo choice = Console.ReadKey();
                

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    if (choice.Key == ConsoleKey.A)
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            int leaguePosition = InsertScore("test" + random.Next(), random.Next(), connection);
                            Console.WriteLine($"Position: {leaguePosition}");
                        }
                    }
                    else if (choice.Key == ConsoleKey.S)
                    {
                        Console.WriteLine("Insert position to query LeagueBoard");
                        string position = Console.ReadLine();
                        Select(position, connection);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadKey();
        }

        private static int InsertScore(string playerId, int score, SqlConnection connection)
        {
            if (!playerId.All(Char.IsLetterOrDigit))
            {
                throw new ArgumentOutOfRangeException(nameof(playerId), "Must only contain numbers and letters");    // To avoid SQL injection
            }
            
            using(SqlCommand cmd = new SqlCommand("INSERT INTO league (playerId, score) output INSERTED.leaguePosition VALUES(@playerId, @score)",connection))
            {
                cmd.Parameters.AddWithValue("@playerId", playerId);
                cmd.Parameters.AddWithValue("@score", score);
                connection.Open();

                int leaguePosition = (int)cmd.ExecuteScalar();

                if (connection.State == System.Data.ConnectionState.Open) 
                    connection.Close();

                return leaguePosition;
            }
        }

        private static void Select(string leaguePosition, SqlConnection connection)
        {
            string startPosition = leaguePosition.Remove((leaguePosition.Length - 2));
            startPosition += "00";

            string sql = "SELECT * FROM league WHERE leaguePosition >= @startRange AND leaguePosition <= @endRange";
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@startRange", int.Parse(startPosition));
                cmd.Parameters.AddWithValue("@endRange", int.Parse(startPosition) + 100);
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        Console.WriteLine($"Position{reader.GetInt32(0)}, PlayerId: {reader.GetString(1)} Score: {reader.GetInt32(2)}");
                    }
                }
                
                if (connection.State == System.Data.ConnectionState.Open) 
                    connection.Close();
            }

        } 
    }
}