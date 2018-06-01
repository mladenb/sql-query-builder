namespace SqlQueryBuilder.Clauses
{
	public class InnerJoinSqlClause : SqlClause
	{
		public string TableName { get; }
		public string OnConditions { get; }
		public object[] Parameters { get; }

		public InnerJoinSqlClause(string tableName, string onConditions, object[] parameters)
		{
			TableName = tableName;
			OnConditions = onConditions;
			Parameters = parameters;
		}
	}
}
