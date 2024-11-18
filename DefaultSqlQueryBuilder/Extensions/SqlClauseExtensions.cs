using DefaultSqlQueryBuilder.Clauses;
using DefaultSqlQueryBuilder.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefaultSqlQueryBuilder.Extensions
{
	public static class SqlClauseExtensions
	{
		public static IEnumerable<ISqlClause> ConsolidateWhereClauses(this IEnumerable<ISqlClause> sqlClauses)
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
			var query = new SqlQuery(first.WhereConditions, first.Parameters);
			var newQuery = query.Append(second.WhereConditions, second.Parameters, "({0}) AND ({1})");

			return new WhereSqlClause(newQuery.Sql, newQuery.Parameters);
		}

		public static IEnumerable<ISqlClause> FixSkipTakeForSqlite(this IEnumerable<ISqlClause> clauses)
		{
			var list = clauses.ToList();
			if (list.Find(c => c is SkipSqlClause) == null) return list;

			if (list.Find(c => c is TakeSqlClause) == null)
			{
				list.Insert(list.FindIndex(c => c is SkipSqlClause), new TakeSqlClause(-1));
				return list;
			}

			var skipIndex = list.FindIndex(c => c is SkipSqlClause);
			var takeIndex = list.FindIndex(c => c is TakeSqlClause);
			if (skipIndex < takeIndex) (list[skipIndex], list[takeIndex]) = (list[takeIndex], list[skipIndex]); // swap

			return list;
		}
	}
}
