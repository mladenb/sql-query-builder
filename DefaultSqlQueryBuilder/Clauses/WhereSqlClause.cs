using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class WhereSqlClause : ISqlClause
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
