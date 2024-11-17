using DefaultSqlQueryBuilder.Clauses;
using DefaultSqlQueryBuilder.Contracts;
using System;

namespace DefaultSqlQueryBuilder.SqlSyntaxes
{
	public abstract class SqlSyntaxBase : ISqlSyntax
	{
		public virtual SqlQuery ToSqlQuery(ISqlClause clause)
		{
			return clause switch
			{
				CustomSqlClause customClause => new SqlQuery(customClause.Sql, customClause.Parameters),
				_ => throw new NotImplementedException(),
			};
		}
	}
}
