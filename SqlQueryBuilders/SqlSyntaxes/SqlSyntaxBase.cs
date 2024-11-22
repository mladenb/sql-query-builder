using SqlQueryBuilders.Clauses;
using SqlQueryBuilders.Contracts;
using System;
using System.Collections.Generic;

namespace SqlQueryBuilders.SqlSyntaxes
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

		public abstract string EscapeTableName(string tableName);
		public abstract string EscapeColumnName(string columnName);
		public abstract IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses);
	}
}
