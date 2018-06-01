namespace SqlQueryBuilder.Clauses
{
	public class CustomSqlClause : SqlClause
	{
		public string Sql { get; }
		public object[] Parameters { get; }

		public CustomSqlClause(string sql, object[] parameters)
		{
			Sql = sql;
			Parameters = parameters;
		}
	}
}
