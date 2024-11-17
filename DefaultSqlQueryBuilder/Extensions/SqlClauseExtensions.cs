using DefaultSqlQueryBuilder.Clauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefaultSqlQueryBuilder.Extensions
{
	public static class SqlClauseExtensions
	{
		public static IEnumerable<SqlClause> ConsolidateWhereClauses(this IEnumerable<SqlClause> sqlClauses)
		{
			var clauses = sqlClauses.ToList();
			var removableWhereClauses = clauses
				.FindAll(clause => clause is WhereSqlClause)
				.Cast<WhereSqlClause>()
				.ToArray();

			if (removableWhereClauses.Length > 1)
			{
				var lastWhereClause = clauses.IndexOf(removableWhereClauses.Last());
				clauses[lastWhereClause] = removableWhereClauses.Aggregate(MergeWhere);
				clauses.RemoveAll(removableWhereClauses.Contains);
			}

			return clauses;
		}

		public static WhereSqlClause MergeWhere(this WhereSqlClause first, WhereSqlClause second)
		{
			var merger = new SqlClauseMerger(first.WhereConditions, first.Parameters, second.WhereConditions, second.Parameters);
			var newClause = merger.Merge("({0}) AND ({1})");

			return new WhereSqlClause(newClause.Sql, newClause.Parameters);
		}

		public static CustomSqlClause Append(this CustomSqlClause source, string sql, params object[] parameters)
		{
			var merger = new SqlClauseMerger(source.Sql, source.Parameters, sql, parameters);
			var newClause = merger.Merge("{0}\n{1}");

			return new CustomSqlClause(newClause.Sql, newClause.Parameters);
		}
	}
}
