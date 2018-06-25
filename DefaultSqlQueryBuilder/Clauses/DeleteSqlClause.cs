namespace DefaultSqlQueryBuilder.Clauses
{
	public class DeleteSqlClause : SqlClause
	{
		public string TableName { get; }

		public DeleteSqlClause(string tableName)
		{
			TableName = tableName;
		}
	}
}
