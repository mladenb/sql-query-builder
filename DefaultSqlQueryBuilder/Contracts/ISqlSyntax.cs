using DefaultSqlQueryBuilder.Clauses;

namespace DefaultSqlQueryBuilder.Contracts
{
	public interface ISqlSyntax
	{
		CustomSqlClause ToSql(SqlClause clause);
	}
}
