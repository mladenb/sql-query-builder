using NPoco;
using SqlQueryBuilder;
using System;
using System.Linq;

namespace NPocoSqlQueryBuilder.Extensions
{
	public static class SqlQueryBuilderBaseExtensions
	{
		public static Sql ToSql(this SqlQueryBuilderBase source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			var sqls = source
				.Clauses
				.ConsolidateWhereClauses()
				.Select(c => c.ToSql());

			var result = Sql.Builder;

			foreach (var sql in sqls)
			{
				result.Append(sql);
			}

			return result;
		}
	}
}
