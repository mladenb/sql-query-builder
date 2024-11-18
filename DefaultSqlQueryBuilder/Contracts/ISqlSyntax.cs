using System.Collections.Generic;

namespace DefaultSqlQueryBuilder.Contracts
{
	public interface ISqlSyntax
	{
		string EscapeTableName(string tableName);
		string EscapeColumnName(string columnName);
		IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses);
	}
}
