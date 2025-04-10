using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using GeminiSqlQueryGenerator.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace Test_AI.Services
{
    public class DatabaseSchemaService
    {
        private readonly string _connectionString;

        public DatabaseSchemaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("Database connection string must be configured in appsettings.json");
            }
        }

        // Phương thức này truy xuất lược đồ CSDL từ SQL Server
        public async Task<DatabaseSchema> GetDatabaseSchemaAsync()
        {
            var schema = new DatabaseSchema
            {
                Tables = new List<TableSchema>(),
                Relationships = new List<Relationship>()
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Lấy danh sách các bảng
                var tables = await GetTablesAsync(connection);

                foreach (var table in tables)
                {
                    // Lấy thông tin các cột cho mỗi bảng
                    var columns = await GetColumnsAsync(connection, table.Name);
                    table.Columns = columns;
                    schema.Tables.Add(table);
                }

                // Lấy danh sách các mối quan hệ
                schema.Relationships = await GetRelationshipsAsync(connection);
            }

            return schema;
        }

        private async Task<List<TableSchema>> GetTablesAsync(SqlConnection connection)
        {
            var tables = new List<TableSchema>();

            using (var command = new SqlCommand(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add(new TableSchema
                        {
                            Name = reader["TABLE_NAME"].ToString(),
                            Columns = new List<ColumnSchema>()
                        });
                    }
                }
            }

            return tables;
        }

        private async Task<List<ColumnSchema>> GetColumnsAsync(SqlConnection connection, string tableName)
        {
            var columns = new List<ColumnSchema>();

            // Lấy thông tin cột
            using (var command = new SqlCommand(
                @"SELECT 
                    c.COLUMN_NAME, 
                    c.DATA_TYPE,
                    CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY
                FROM 
                    INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN (
                    SELECT ku.TABLE_CATALOG, ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                        ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                        AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                ) pk
                ON  c.TABLE_CATALOG = pk.TABLE_CATALOG
                    AND c.TABLE_SCHEMA = pk.TABLE_SCHEMA
                    AND c.TABLE_NAME = pk.TABLE_NAME
                    AND c.COLUMN_NAME = pk.COLUMN_NAME
                WHERE c.TABLE_NAME = @TableName",
                connection))
            {
                command.Parameters.AddWithValue("@TableName", tableName);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        columns.Add(new ColumnSchema
                        {
                            Name = reader["COLUMN_NAME"].ToString(),
                            DataType = reader["DATA_TYPE"].ToString(),
                            IsPrimaryKey = Convert.ToBoolean(reader["IS_PRIMARY_KEY"]),
                            IsForeignKey = false
                        });
                    }
                }
            }

            return columns;
        }

        private async Task<List<Relationship>> GetRelationshipsAsync(SqlConnection connection)
        {
            var relationships = new List<Relationship>();

            using (var command = new SqlCommand(
                @"SELECT 
                    fk.name AS FK_NAME,
                    tp.name AS PARENT_TABLE,
                    cp.name AS PARENT_COLUMN,
                    tr.name AS REFERENCED_TABLE,
                    cr.name AS REFERENCED_COLUMN
                FROM 
                    sys.foreign_keys fk
                INNER JOIN 
                    sys.tables tp ON fk.parent_object_id = tp.object_id
                INNER JOIN 
                    sys.tables tr ON fk.referenced_object_id = tr.object_id
                INNER JOIN 
                    sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                INNER JOIN 
                    sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
                INNER JOIN 
                    sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        relationships.Add(new Relationship
                        {
                            SourceTable = reader["PARENT_TABLE"].ToString(),
                            SourceColumn = reader["PARENT_COLUMN"].ToString(),
                            TargetTable = reader["REFERENCED_TABLE"].ToString(),
                            TargetColumn = reader["REFERENCED_COLUMN"].ToString()
                        });
                    }
                }
            }

            return relationships;
        }

        // Chuyển đổi schema thành chuỗi JSON
        public string GetDatabaseSchemaAsJson(DatabaseSchema schema)
        {
            return JsonConvert.SerializeObject(schema, Formatting.Indented);
        }
    }
}
