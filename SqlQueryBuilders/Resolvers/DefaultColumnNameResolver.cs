using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Resolvers
{
	public class DefaultColumnNameResolver : IColumnNameResolver
	{
		public string Resolve(string memberName) => memberName;
	}
}
