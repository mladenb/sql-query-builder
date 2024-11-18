using DefaultSqlQueryBuilder.Clauses;
using DefaultSqlQueryBuilder.Contracts;
using DefaultSqlQueryBuilder.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DefaultSqlQueryBuilder.SqlSyntaxes
{
	public class SQLiteSyntax : MsSqlSyntax
	{
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
