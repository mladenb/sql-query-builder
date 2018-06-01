namespace SqlQueryBuilder.Clauses
{
	public class DeleteSqlClause : SqlClause
	{
		public string TableName { get; }
		public object[] Parameters { get; }

		public DeleteSqlClause(string tableName, object[] parameters)
		{
			TableName = tableName;
			Parameters = parameters;
		}
	}
}
