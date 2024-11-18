using System.Collections.Generic;

namespace DefaultSqlQueryBuilder.Contracts
{
	public interface ISqlSyntax
	{
		IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses);
	}
}
