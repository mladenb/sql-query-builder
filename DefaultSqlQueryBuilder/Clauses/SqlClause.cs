using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class SqlClause : ISqlClause
	{
		public string Sql { get; }
		public object[] Parameters { get; }

		public SqlClause(string sql, params object[] parameters)
		{
			Sql = sql;
			Parameters = parameters;
		}
	}
}
