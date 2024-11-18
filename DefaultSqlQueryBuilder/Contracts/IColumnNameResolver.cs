namespace DefaultSqlQueryBuilder.Contracts
{
	public interface IColumnNameResolver
	{
		string Resolve(string memberName);
	}
}
