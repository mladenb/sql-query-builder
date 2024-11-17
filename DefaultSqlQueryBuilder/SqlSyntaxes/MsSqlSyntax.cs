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
			return clause switch
			{
				WhereSqlClause whereClause => new SqlClause($"WHERE ({whereClause.WhereConditions})", whereClause.Parameters),
				UpdateSqlClause updateClause => new SqlClause($"UPDATE {updateClause.TableName} SET {updateClause.ColumnsWithValues}", updateClause.Parameters),
				SelectSqlClause selectClause => new SqlClause($"SELECT {selectClause.Columns}"),
				OrderBySqlClause orderByClause => new SqlClause($"ORDER BY {orderByClause.Columns}"),
				LeftJoinSqlClause leftJoinClause => new SqlClause($"LEFT JOIN {leftJoinClause.TableName} ON {leftJoinClause.OnConditions}", leftJoinClause.Parameters),
				InsertSqlClause insertClause => new SqlClause($"INSERT INTO {insertClause.TableName} ({insertClause.Columns}) VALUES ({ToPlaceholdersCsv(insertClause.Parameters)})", insertClause.Parameters),
				InsertMultipleSqlClause insertMultipleClause => CreateInsertMultiple(insertMultipleClause),
				InnerJoinSqlClause innerJoinClause => new SqlClause($"INNER JOIN {innerJoinClause.TableName} ON {innerJoinClause.OnConditions}", innerJoinClause.Parameters),
				GroupBySqlClause groupByClause => new SqlClause($"GROUP BY {groupByClause.Columns}"),
				FromSqlClause fromClause => new SqlClause($"FROM {fromClause.TableName}"),
				DeleteSqlClause deleteClause => new SqlClause($"DELETE FROM {deleteClause.TableName}"),
				SqlClause customClause => customClause,
				_ => throw new NotImplementedException(),
			};
		}

		private SqlClause CreateInsertMultiple(InsertMultipleSqlClause insertMultipleClause)
		{
			var monkeys = GetInsertMultipleMonkeys(insertMultipleClause.Parameters);
			var values = GetInsertMultipleValues(insertMultipleClause.Parameters);

			return new SqlClause($"INSERT INTO {insertMultipleClause.TableName} ({insertMultipleClause.Columns}) VALUES {monkeys}", values);
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
