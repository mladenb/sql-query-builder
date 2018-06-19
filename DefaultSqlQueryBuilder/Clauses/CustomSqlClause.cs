using System;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class CustomSqlClause : SqlClause
	{
		public string Sql { get; }
		public object[] Parameters { get; }

		public CustomSqlClause(string sql, object[] parameters = null)
		{
			Sql = sql ?? throw new ArgumentNullException(nameof(sql));
			Parameters = parameters ?? new object[0];
		}
	}
}
