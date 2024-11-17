using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
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
