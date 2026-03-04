using SessionLogger.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SessionLogger.Controllers;

internal class DatabaseController
{
    private IConfiguration setConfig()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration config = builder.Build();

        return config;
    }

    public void SQLHandler(string query)
    {
        var config = setConfig();
        var connection = new SqliteConnection($"Data Source={config["connectionstring:DataSource"]}");

        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = query;
        
        command.ExecuteNonQuery();

        connection.Close(); 
    }
     
    public List<Session> GetAllRecords(DateTime? dateTimeFilter = null)
    {
        var rows = new List<Session>();
        
        var config = setConfig();
        var connection = new SqliteConnection($"Data Source={config["connectionstring:DataSource"]}");

        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, type, date, start, end FROM sessions
            ORDER BY date ASC, start ASC;
        """;
        command.ExecuteNonQuery();
        SqliteDataReader datareader;
        datareader = command.ExecuteReader();
        var i=0;
        
        while (datareader.Read()){
            if (!dateTimeFilter.HasValue | (dateTimeFilter.HasValue & DateTime.Parse(datareader.GetString(2)) > dateTimeFilter)) {
                rows.Add(new Session(datareader.GetString(1), DateTime.Parse(datareader.GetString(2)), DateTime.Parse(datareader.GetString(3)),  DateTime.Parse(datareader.GetString(4))));
                rows[i].Id = datareader.GetInt16(0);
                i++;
            }
            }      
        
        connection.Close();
        return rows;
    }
}