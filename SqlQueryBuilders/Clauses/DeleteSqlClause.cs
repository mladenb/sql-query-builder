using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Clauses
{
	public class DeleteSqlClause : ISqlClause
	{
		public string TableName { get; }

		public DeleteSqlClause(string tableName)
		{
			TableName = tableName;
		}
	}
}
