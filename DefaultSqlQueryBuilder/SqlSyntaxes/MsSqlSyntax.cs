using DefaultSqlQueryBuilder.Clauses;
using DefaultSqlQueryBuilder.Contracts;
using DefaultSqlQueryBuilder.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DefaultSqlQueryBuilder.SqlSyntaxes
{
	public class MsSqlSyntax : SqlSyntaxBase
	{
		public override IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses) => clauses.Select(ToSqlQuery);

		protected override SqlQuery ToSqlQuery(ISqlClause clause)
		{
			return clause switch
			{
				WhereSqlClause whereClause => new SqlQuery($"WHERE ({whereClause.WhereConditions})", whereClause.Parameters),
				UpdateSqlClause updateClause => new SqlQuery($"UPDATE {updateClause.TableName} SET {updateClause.ColumnsWithValues}", updateClause.Parameters),
				SelectSqlClause selectClause => new SqlQuery($"SELECT {selectClause.Columns}"),
				OrderBySqlClause orderByClause => new SqlQuery($"ORDER BY {orderByClause.Columns}{GetOrderByDirection(orderByClause)}"),
				SkipSqlClause skipClause => new SqlQuery($"OFFSET {skipClause.RowCount} ROWS"),
				TakeSqlClause takeClause => new SqlQuery($"FETCH NEXT {takeClause.RowCount} ROWS ONLY"),
				LeftJoinSqlClause leftJoinClause => new SqlQuery($"LEFT JOIN {leftJoinClause.TableName} ON {leftJoinClause.OnConditions}", leftJoinClause.Parameters),
				InsertSqlClause insertClause => new SqlQuery($"INSERT INTO {insertClause.TableName} ({insertClause.Columns}) VALUES ({ToPlaceholdersCsv(insertClause.Parameters)})", insertClause.Parameters),
				InsertMultipleSqlClause insertMultipleClause => CreateInsertMultiple(insertMultipleClause),
				InnerJoinSqlClause innerJoinClause => new SqlQuery($"INNER JOIN {innerJoinClause.TableName} ON {innerJoinClause.OnConditions}", innerJoinClause.Parameters),
				GroupBySqlClause groupByClause => new SqlQuery($"GROUP BY {groupByClause.Columns}"),
				FromSqlClause fromClause => new SqlQuery($"FROM {fromClause.TableName}"),
				DeleteSqlClause deleteClause => new SqlQuery($"DELETE FROM {deleteClause.TableName}"),
				_ => base.ToSqlQuery(clause),
			};
		}

		private string GetOrderByDirection(OrderBySqlClause clause)
		{
			return clause.OrderingDirection != OrderingDirection.Ascending
				? " DESC"
				: "";
		}

		private SqlQuery CreateInsertMultiple(InsertMultipleSqlClause insertMultipleClause)
		{
			var monkeys = GetInsertMultipleMonkeys(insertMultipleClause.Parameters);
			var values = GetInsertMultipleValues(insertMultipleClause.Parameters);

			return new SqlQuery($"INSERT INTO {insertMultipleClause.TableName} ({insertMultipleClause.Columns}) VALUES {monkeys}", values);
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
