using System.Collections.Generic;

namespace GeminiSqlQueryGenerator.Models
{
    public class TableSchema
    {
        public string Name { get; set; }
        public List<ColumnSchema> Columns { get; set; }
    }

    public class ColumnSchema
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
    }

    public class DatabaseSchema
    {
        public List<TableSchema> Tables { get; set; }
        public List<Relationship> Relationships { get; set; }
    }

    public class Relationship
    {
        public string SourceTable { get; set; }
        public string SourceColumn { get; set; }
        public string TargetTable { get; set; }
        public string TargetColumn { get; set; }
    }
}
