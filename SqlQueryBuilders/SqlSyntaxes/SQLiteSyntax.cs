using SqlQueryBuilders.Clauses;
using SqlQueryBuilders.Contracts;
using SqlQueryBuilders.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryBuilders.SqlSyntaxes
{
	public class SQLiteSyntax : MsSqlSyntax
	{
		public override string EscapeTableName(string tableName) => $"\"{tableName}\"";
		public override string EscapeColumnName(string columnName) => $"\"{columnName}\"";

		public override IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses)
		{
			return clauses
				.FixSkipTakeForSqlite()
				.Select(ToSqlQuery);
		}

		protected override SqlQuery ToSqlQuery(ISqlClause clause)
		{
			return clause switch
			{
				SkipSqlClause skipClause => new SqlQuery($"OFFSET {skipClause.RowCount}"),
				TakeSqlClause takeClause => new SqlQuery($"LIMIT {takeClause.RowCount}"),
				_ => base.ToSqlQuery(clause),
			};
		}
	}
}
