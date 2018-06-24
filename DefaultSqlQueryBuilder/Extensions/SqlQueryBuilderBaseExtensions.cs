using System;
using System.Linq;

namespace DefaultSqlQueryBuilder.Extensions
{
	public static class SqlQueryBuilderBaseExtensions
	{
		public static SqlQuery ToSqlQuery(this SqlQueryBuilderBase source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			var sqls = source
				.Clauses
				.ConsolidateWhereClauses()
				.Select(c => c.ToSql());

			var result = sqls.Aggregate((current, sql) => current.Append(sql.Sql, sql.Parameters));

			return new SqlQuery(result.Sql, result.Parameters);
		}
	}
}
