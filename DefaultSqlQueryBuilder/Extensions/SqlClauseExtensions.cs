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
	}
}
