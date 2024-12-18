﻿using System.Collections.Generic;

namespace SqlQueryBuilders.Contracts
{
	public interface ISqlSyntax
	{
		string EscapeTableName(string tableName);
		string EscapeColumnName(string columnName);
		IEnumerable<SqlQuery> ToSqlQuery(IEnumerable<ISqlClause> clauses);
	}
}
