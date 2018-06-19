namespace DefaultSqlQueryBuilder.Clauses
{
	public class FromSqlClause : SqlClause
	{
		public string TableName { get; }

		public FromSqlClause(string tableName)
		{
			TableName = tableName;
		}
	}
}
