namespace SqlQueryBuilders.Contracts
{
	public interface IColumnNameResolver
	{
		string Resolve(string memberName);
	}
}
