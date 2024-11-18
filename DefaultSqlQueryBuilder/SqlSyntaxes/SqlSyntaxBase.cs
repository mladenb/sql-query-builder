using DefaultSqlQueryBuilder.Clauses;
using DefaultSqlQueryBuilder.Contracts;
using System;
using System.Collections.Generic;

namespace DefaultSqlQueryBuilder.SqlSyntaxes
{
	public abstract class SqlSyntaxBase : ISqlSyntax
	{
		protected virtual SqlQuery ToSqlQuery(ISqlClause clause)
		{
			return clause switch
			{
				CustomSqlClause customClause => new SqlQuery(customClause.Sql, customClause.Parameters),
				_ => throw new NotImplementedException(),
			};
		}

		public abstract IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses);
	}
}
