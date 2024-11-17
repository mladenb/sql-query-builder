using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class FromSqlClause : ISqlClause
	{
		public string TableName { get; }

		public FromSqlClause(string tableName)
		{
			TableName = tableName;
		}
	}
}
