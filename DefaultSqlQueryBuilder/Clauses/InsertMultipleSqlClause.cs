using DefaultSqlQueryBuilder.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class InsertMultipleSqlClause : ISqlClause
	{
		public string TableName { get; }
		public string Columns { get; }
		public object[][] Parameters { get; }

		public InsertMultipleSqlClause(string tableName, string columns, IEnumerable<object[]> parameters)
		{
			TableName = tableName;
			Columns = columns;
			Parameters = parameters.ToArray();
		}
	}
}
