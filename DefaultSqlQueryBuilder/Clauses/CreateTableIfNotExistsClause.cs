using DefaultSqlQueryBuilder.Contracts;
using System.Collections.Generic;
using System.Reflection;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class CreateTableIfNotExistsClause : ISqlClause
	{
		public string TableName { get; }
		public IReadOnlyDictionary<string, PropertyInfo> Columns { get; }

		public CreateTableIfNotExistsClause(string tableName, IReadOnlyDictionary<string, PropertyInfo> columns)
		{
			TableName = tableName;
			Columns = columns;
		}
	}
}
