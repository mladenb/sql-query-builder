using NPoco;
using SqlQueryBuilder.Clauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NPocoSqlQueryBuilder.Extensions
{
	public static class SqlClauseExtensions
	{
		public static Sql ToSql(this SqlClause source)
		{
			switch (source)
			{
				case WhereSqlClause whereClause: 
					return Sql.Builder.Where(whereClause.WhereConditions, whereClause.Parameters);

				case UpdateSqlClause updateClause:
					return Sql.Builder.Append($"UPDATE {updateClause.TableName} SET {updateClause.ColumnsWithValues}", updateClause.Parameters);

				case SelectSqlClause selectClause:
					return Sql.Builder.Select(selectClause.Columns);

				case OrderBySqlClause orderByClause:
					return Sql.Builder.OrderBy(orderByClause.Columns);

				case LeftJoinSqlClause leftJoinClause:
					return Sql.Builder
						.LeftJoin(leftJoinClause.TableName)
						.On(leftJoinClause.OnConditions, leftJoinClause.Parameters);

				case InsertSqlClause insertClause:
					return Sql.Builder.Append($"INSERT INTO {insertClause.TableName} ({insertClause.Columns}) VALUES ({ToPlaceholdersCsv(insertClause.Parameters)})", insertClause.Parameters);

				case InnerJoinSqlClause innerJoinClause:
					return Sql.Builder
						.InnerJoin(innerJoinClause.TableName)
						.On(innerJoinClause.OnConditions, innerJoinClause.Parameters);

				case GroupBySqlClause groupByClause:
					return Sql.Builder.GroupBy(groupByClause.Columns);

				case FromSqlClause fromClause:
					return Sql.Builder.From(fromClause.TableName);

				case DeleteSqlClause deleteClause:
					return Sql.Builder.Append($"DELETE FROM {deleteClause.TableName}", deleteClause.Parameters);

				case CustomSqlClause customClause:
					return Sql.Builder.Append(customClause.Sql, customClause.Parameters);
			}

			throw new NotImplementedException();
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

		/// <summary>
		/// Given the list of parameters, returns the CSV list of SQL parameter placeholders (@0, @1, @2, ...)
		/// </summary>
		private static string ToPlaceholdersCsv(IEnumerable<object> parameters)
		{
			var monkeys = parameters
				.Select((o, i) => $"@{i}");

			return string.Join(", ", monkeys);
		}
	}
}
