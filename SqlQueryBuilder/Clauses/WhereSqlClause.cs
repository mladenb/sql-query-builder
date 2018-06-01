namespace SqlQueryBuilder.Clauses
{
	public class WhereSqlClause : SqlClause
	{
		public WhereSqlClause(string whereConditions, object[] parameters)
		{
			WhereConditions = whereConditions;
			Parameters = parameters;
		}

		public string WhereConditions { get; }
		public object[] Parameters { get; }
	}
}
