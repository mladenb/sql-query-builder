using DefaultSqlQueryBuilder.Clauses;
using System;
using System.Linq;

namespace DefaultSqlQueryBuilder.Extensions
{
	public static class SqlQueryBuilderBaseExtensions
	{
		public static CustomSqlClause ToSql(this SqlQueryBuilderBase source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			var sqls = source
				.Clauses
				.ConsolidateWhereClauses()
				.Select(c => c.ToSql());

			return sqls.Aggregate((current, sql) => current.Append(sql.Sql, sql.Parameters));
		}
	}
}
