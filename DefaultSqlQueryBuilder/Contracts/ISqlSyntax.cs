using DefaultSqlQueryBuilder.Clauses;

namespace DefaultSqlQueryBuilder.Contracts
{
	public interface ISqlSyntax
	{
		SqlClause ToSql(ISqlClause clause);
	}
}
