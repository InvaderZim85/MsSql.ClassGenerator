using Dapper;
using Microsoft.Data.SqlClient;

namespace MsSql.ClassGenerator.Core.Data;

/// <summary>
/// Provides the basic functions for the interaction with the database.
/// </summary>
/// <param name="server">The name / path of the MS SQL server.</param>
/// <param name="database">The name of the desired database.</param>
public class BaseRepo(string server, string database = "")
{
    /// <summary>
    /// Loads all available databases of the selected server
    /// </summary>
    /// <returns>The list with the databases</returns>
    public async Task<List<string>> LoadDatabasesAsync()
    {
        return await QueryAsListAsync<string>("SELECT [name] FROM sys.databases ORDER BY [name]");
    }

    /// <summary>
    /// Gets the connection string which is needed for the connection with the database.
    /// </summary>
    /// <returns></returns>
    protected SqlConnection GetConnection()
    {
        var conString = new SqlConnectionStringBuilder
        {
            DataSource = server,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ApplicationName = nameof(ClassGenerator)
        };

        if (!string.IsNullOrWhiteSpace(database))
            conString.InitialCatalog = database;

        return new SqlConnection(conString.ConnectionString);
    }

    /// <summary>
    /// Reads the data of the reader for the specified type and returns the result as a list.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="reader">The grid reader.</param>
    /// <returns>The list with the data.</returns>
    protected static async Task<List<T>> ReadListAsync<T>(SqlMapper.GridReader reader)
    {
        var data = await reader.ReadAsync<T>();

        return data.ToList();
    }

    /// <summary>
    /// Executes the specified query and returns the result as a list.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="query">The query which should be executed.</param>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The result.</returns>
    protected async Task<List<T>> QueryAsListAsync<T>(string query, object? parameter = null)
    {
        await using var connection = GetConnection();

        var result = await connection.QueryAsync<T>(query, parameter);

        return result.ToList();
    }
}
