using DefaultSqlQueryBuilder.Clauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefaultSqlQueryBuilder.Extensions
{
	public static class SqlClauseExtensions
	{
		public static CustomSqlClause ToSql(this SqlClause source)
		{
			switch (source)
			{
				case WhereSqlClause whereClause:
					return new CustomSqlClause($"WHERE ({whereClause.WhereConditions})", whereClause.Parameters);

				case UpdateSqlClause updateClause:
					return new CustomSqlClause($"UPDATE {updateClause.TableName} SET {updateClause.ColumnsWithValues}", updateClause.Parameters);

				case SelectSqlClause selectClause:
					return new CustomSqlClause($"SELECT {selectClause.Columns}");

				case OrderBySqlClause orderByClause:
					return new CustomSqlClause($"ORDER BY {orderByClause.Columns}");

				case LeftJoinSqlClause leftJoinClause:
					return new CustomSqlClause($"LEFT JOIN {leftJoinClause.TableName} ON {leftJoinClause.OnConditions}", leftJoinClause.Parameters);

				case InsertSqlClause insertClause:
					return new CustomSqlClause($"INSERT INTO {insertClause.TableName} ({insertClause.Columns}) VALUES ({ToPlaceholdersCsv(insertClause.Parameters)})", insertClause.Parameters);

				case InsertMultipleSqlClause insertMultipleClause:
					var monkeys = GetInsertMultipleMonkeys(insertMultipleClause.Parameters);
					var values = GetInsertMultipleValues(insertMultipleClause.Parameters);
					return new CustomSqlClause($"INSERT INTO {insertMultipleClause.TableName} ({insertMultipleClause.Columns}) VALUES {monkeys}", values);

				case InnerJoinSqlClause innerJoinClause:
					return new CustomSqlClause($"INNER JOIN {innerJoinClause.TableName} ON {innerJoinClause.OnConditions}", innerJoinClause.Parameters);

				case GroupBySqlClause groupByClause:
					return new CustomSqlClause($"GROUP BY {groupByClause.Columns}");

				case FromSqlClause fromClause:
					return new CustomSqlClause($"FROM {fromClause.TableName}");

				case DeleteSqlClause deleteClause:
					return new CustomSqlClause($"DELETE FROM {deleteClause.TableName}");

				case CustomSqlClause customClause:
					return customClause;
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

			var merger = new SqlClauseMerger(first.WhereConditions, first.Parameters, second.WhereConditions, second.Parameters);
			var newClause = merger.Merge("({0}) AND ({1})");

			return new WhereSqlClause(newClause.Sql, newClause.Parameters);
		}

		public static CustomSqlClause Append(this CustomSqlClause source, string sql, object[] parameters = null)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (sql == null) throw new ArgumentNullException(nameof(sql));

			if (parameters == null) parameters = new object[0];

			var merger = new SqlClauseMerger(source.Sql, source.Parameters, sql, parameters);
			var newClause = merger.Merge("{0}\n{1}");

			return new CustomSqlClause(newClause.Sql, newClause.Parameters);
		}

		private static object[] GetInsertMultipleValues(IEnumerable<IEnumerable<object>> parameters)
		{
			return parameters
				.SelectMany(o => o)
				.ToArray();
		}

		private static string GetInsertMultipleMonkeys(IEnumerable<object[]> rows)
		{
			var values = rows.Select((row, index) => "(" + ToPlaceholdersCsv(row, index * row.Length) + ")");

			return string.Join(", ", values);
		}

		/// <summary>
		/// Given the list of parameters, returns the CSV list of SQL parameter placeholders (@0, @1, @2, ...)
		/// </summary>
		private static string ToPlaceholdersCsv(IEnumerable<object> parameters, int startFrom = 0)
		{
			var monkeys = parameters
				.Select((o, i) => $"@{startFrom + i}");

			return string.Join(", ", monkeys);
		}
	}
}
