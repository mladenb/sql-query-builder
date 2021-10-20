using System;

namespace DefaultSqlQueryBuilder
{
    /// <summary>
    /// Database Table Name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Cannot be empty string", nameof(tableName));

            TableName = tableName;
        }

        public string TableName { get; set; }
    }
}