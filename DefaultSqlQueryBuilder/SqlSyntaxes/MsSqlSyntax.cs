using DefaultSqlQueryBuilder.Clauses;
using DefaultSqlQueryBuilder.Contracts;
using DefaultSqlQueryBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefaultSqlQueryBuilder.SqlSyntaxes
{
	public class MsSqlSyntax : ISqlSyntax
	{
		public SqlClause ToSql(ISqlClause clause)
		{
			switch (clause)
			{
				case WhereSqlClause whereClause:
					return new SqlClause($"WHERE ({whereClause.WhereConditions})", whereClause.Parameters);

				case UpdateSqlClause updateClause:
					return new SqlClause($"UPDATE {updateClause.TableName} SET {updateClause.ColumnsWithValues}", updateClause.Parameters);

				case SelectSqlClause selectClause:
					return new SqlClause($"SELECT {selectClause.Columns}");

				case OrderBySqlClause orderByClause:
					return new SqlClause($"ORDER BY {orderByClause.Columns}");

				case LeftJoinSqlClause leftJoinClause:
					return new SqlClause($"LEFT JOIN {leftJoinClause.TableName} ON {leftJoinClause.OnConditions}", leftJoinClause.Parameters);

				case InsertSqlClause insertClause:
					return new SqlClause($"INSERT INTO {insertClause.TableName} ({insertClause.Columns}) VALUES ({ToPlaceholdersCsv(insertClause.Parameters)})", insertClause.Parameters);

				case InsertMultipleSqlClause insertMultipleClause:
					var monkeys = GetInsertMultipleMonkeys(insertMultipleClause.Parameters);
					var values = GetInsertMultipleValues(insertMultipleClause.Parameters);
					return new SqlClause($"INSERT INTO {insertMultipleClause.TableName} ({insertMultipleClause.Columns}) VALUES {monkeys}", values);

				case InnerJoinSqlClause innerJoinClause:
					return new SqlClause($"INNER JOIN {innerJoinClause.TableName} ON {innerJoinClause.OnConditions}", innerJoinClause.Parameters);

				case GroupBySqlClause groupByClause:
					return new SqlClause($"GROUP BY {groupByClause.Columns}");

				case FromSqlClause fromClause:
					return new SqlClause($"FROM {fromClause.TableName}");

				case DeleteSqlClause deleteClause:
					return new SqlClause($"DELETE FROM {deleteClause.TableName}");

				case SqlClause customClause:
					return customClause;
			}

			throw new NotImplementedException();
		}

		private static object[] GetInsertMultipleValues(IEnumerable<IEnumerable<object>> parameters)
		{
			return parameters
				.SelectMany(o => o)
				.ToArray();
		}

		private static string GetInsertMultipleMonkeys(IEnumerable<object[]> rows)
			=> rows
				.Select((row, index) => "(" + ToPlaceholdersCsv(row, index * row.Length) + ")")
				.ToCsv(", ");

		private static string ToPlaceholdersCsv(IEnumerable<object> parameters, int startFrom = 0)
			=> parameters
				.Select((o, i) => $"@{startFrom + i}")
				.ToCsv(", ");
	}
}
