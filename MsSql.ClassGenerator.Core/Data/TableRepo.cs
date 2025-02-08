using Dapper;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model;

namespace MsSql.ClassGenerator.Core.Data;

/// <summary>
/// Provides the functions for the interaction with the table data of the specified database.
/// </summary>
/// <param name="server">The name / path of the MS SQL server.</param>
/// <param name="database">The name of the desired database.</param>
public sealed class TableRepo(string server, string database) : BaseRepo(server, database)
{
    /// <summary>
    /// Loads all available user tables of the specified database.
    /// </summary>
    /// <param name="filter">The desired filter.</param>
    /// <returns>The list with the tables.</returns>
    public async Task<List<TableEntry>> LoadTablesAsync(string filter)
    {
        const string query =
            """
            -- Prepare the temp. tables
            -- Cleaning
            DROP TABLE IF EXISTS #tmpTableIds;
            
            CREATE TABLE #tmpTableIds
            (
                Id INT NOT NULL PRIMARY KEY
            );
            
            -- Load all desired tables (only user tables)
            INSERT INTO #tmpTableIds
            SELECT
                t.object_id AS Id
            FROM
                sys.tables AS t
            WHERE
                t.is_ms_shipped = 0; -- Only user tables
            
            -- Return the tables
            SELECT
                t.object_id AS [Id],
                t.[name],
                s.name AS [Schema]
            FROM
                sys.tables AS t
            
                INNER JOIN #tmpTableIds AS ti
                ON ti.Id = t.object_id
            
                INNER JOIN sys.schemas AS s
                ON s.schema_id = t.schema_id;
            
            -- Return the columns of the tables
            SELECT
                ti.Id AS TableId,
                c.[name],
                c.column_id AS [Order],
                ct.[name] AS DataType,
                COALESCE(ci.CHARACTER_MAXIMUM_LENGTH, c.max_length) AS [MaxLength],
                c.is_nullable AS IsNullable,
                ISNULL(OBJECT_DEFINITION(c.default_object_id), 'NULL') AS DefaultValue
            FROM
                sys.columns AS c
            
                INNER JOIN sys.tables AS t
                ON t.object_id = c.object_id
            
                INNER JOIN #tmpTableIds AS ti
                ON ti.Id = t.object_id
            
                INNER JOIN INFORMATION_SCHEMA.COLUMNS AS ci
                ON ci.TABLE_NAME = t.[name]
                AND ci.COLUMN_NAME = c.[name]
            
                INNER JOIN sys.types AS ct
                ON ct.user_type_id = c.user_type_id;
            """;

        await using var connection = GetConnection();
        var reader = await connection.QueryMultipleAsync(query);

        // Extract the tables
        var tables = await ReadListAsync<TableEntry>(reader);
        var columns = await ReadListAsync<ColumnEntry>(reader);

        var result = new List<TableEntry>();
        // Combine the data
        foreach (var table in tables.Where(w => w.Name.MatchFilter(filter)))
        {
            // Attach the columns.
            table.Columns = columns.Where(f => f.TableId == table.Id).ToList();

            result.Add(table);
        }

        return result;
    }

    /// <summary>
    /// Loads the primary key information of the specified table.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>The awaitable task.</returns>
    public async Task LoadPrimaryKeyInfoAsync(TableEntry table)
    {
        const string query =
            """
            DECLARE @pkValues TABLE
            (
                [Database] SYSNAME NOT NULL,
                [Schema] SYSNAME NOT NULL,
                [Table] SYSNAME NOT NULL,
                [Column] SYSNAME NOT NULL,
                [KeySeq] SMALLINT NOT NULL,
                [PkName] SYSNAME NOT NULL
            );

            INSERT INTO @pkValues
            EXEC sp_pkeys @table_name = @name, @table_owner = @schema;

            SELECT
                [Column]
            FROM
                @pkValues;
            """;

        var columns = await QueryAsListAsync<string>(query, table);

        if (columns.Count == 0)
            return;

        foreach (var column in table.Columns)
        {
            column.IsPrimaryKey = columns.Contains(column.Name);
        }
    }
}
