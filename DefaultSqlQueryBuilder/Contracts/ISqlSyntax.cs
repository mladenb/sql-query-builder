namespace DefaultSqlQueryBuilder.Contracts
{
	public interface ISqlSyntax
	{
		SqlQuery ToSqlQuery(ISqlClause clause);
	}
}
