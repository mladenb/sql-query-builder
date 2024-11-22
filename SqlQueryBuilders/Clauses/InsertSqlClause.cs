using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Clauses
{
	public class InsertSqlClause : ISqlClause
	{
		public string TableName { get; }
		public string Columns { get; }
		public object[] Parameters { get; }

		public InsertSqlClause(string tableName, string columns, object[] parameters)
		{
			TableName = tableName;
			Columns = columns;
			Parameters = parameters;
		}
	}
}
