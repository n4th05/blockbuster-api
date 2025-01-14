using System.Data;
using Dapper;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IDbConnection connection)
    {
        var scripts = Directory
            .GetFiles("Infrastructure/Data/Scripts", "*.sql")
            .OrderBy(f => f)
            .ToList();

        foreach (var script in scripts)
        {
            string sqlScript = await File.ReadAllTextAsync(script);
            await connection.ExecuteAsync(new CommandDefinition(sqlScript));
        }
    }
}