using NPoco;
using SqlQueryBuilder;
using SqlQueryBuilder.Clauses;
using System;
using System.Collections.Generic;
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

		public static IEnumerable<SqlClause> ConsolidateWhereClauses(this IEnumerable<SqlClause> sqlClauses)
		{
			if (sqlClauses == null) throw new ArgumentNullException(nameof(sqlClauses));

			var clauses = sqlClauses.ToList();
			var removableWhereClauses = clauses
				.FindAll(clause => clause is WhereSqlClause)
				.Cast<WhereSqlClause>()
				.ToArray();

			if (removableWhereClauses.Any())
			{
				clauses.RemoveAll(removableWhereClauses.Contains);
				clauses.Add(removableWhereClauses.Aggregate(MergeWhere));
			}

			return clauses;
		}

		public static WhereSqlClause MergeWhere(this WhereSqlClause first, WhereSqlClause second)
		{
			if (first == null) throw new ArgumentNullException(nameof(first));
			if (second == null) throw new ArgumentNullException(nameof(second));

			var sql = Sql.Builder
				.Append($"({first.WhereConditions})", first.Parameters)
				.Append($" AND ({second.WhereConditions})", second.Parameters);

			return new WhereSqlClause(sql.SQL, sql.Arguments);
		}
	}
}
