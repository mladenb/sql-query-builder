namespace DefaultSqlQueryBuilder.Clauses
{
	public class LeftJoinSqlClause : SqlClause
	{
		public string TableName { get; }
		public string OnConditions { get; }
		public object[] Parameters { get; }

		public LeftJoinSqlClause(string tableName, string onConditions, object[] parameters)
		{
			TableName = tableName;
			OnConditions = onConditions;
			Parameters = parameters;
		}
	}
}
