﻿using SqlQueryBuilders.Clauses;
using SqlQueryBuilders.Contracts;
using SqlQueryBuilders.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlQueryBuilders.SqlSyntaxes
{
	public class MsSqlSyntax : SqlSyntaxBase
	{
		public override IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses) => clauses.Select(ToSqlQuery);

		public override string EscapeTableName(string tableName) => $"[{tableName}]";
		public override string EscapeColumnName(string columnName) => $"[{columnName}]";

		private static readonly Dictionary<Type, string> SqlTypeMapping = new Dictionary<Type, string>
		{
			{ typeof(string), "NVARCHAR NOT NULL" },
			{ typeof(int), "INT NOT NULL" },
			{ typeof(int?), "INT" },
			{ typeof(DateTime), "DATETIME2 NOT NULL" },
			{ typeof(DateTime?), "DATETIME2" },
			{ typeof(double), "FLOAT NOT NULL" },
			{ typeof(double?), "FLOAT" },
		};

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
				CreateTableIfNotExistsClause createTableClause => CreateTableIfNotExistsQuery(createTableClause),
				_ => base.ToSqlQuery(clause),
			};
		}

		private SqlQuery CreateTableIfNotExistsQuery(CreateTableIfNotExistsClause clause)
		{
			var sql = string.Join("\n",
				$"IF OBJECT_ID(N'{clause.TableName}', N'U') IS NULL",
				"BEGIN",
				$"CREATE TABLE {EscapeTableName(clause.TableName)}",
				"(",
				string.Join(", ", clause.Columns.Select(GetCreateTableColumn)),
				");",
				"END;"
			);

			return new SqlQuery(sql);
		}

		private string GetCreateTableColumn(KeyValuePair<string, PropertyInfo> col)
		{
			var name = col.Key;
			var type = col.Value.PropertyType;
			if (!SqlTypeMapping.ContainsKey(type)) throw new NotImplementedException();

			return $"{name} {SqlTypeMapping[type]}";
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
