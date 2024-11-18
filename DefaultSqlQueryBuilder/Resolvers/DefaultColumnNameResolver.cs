using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Resolvers
{
	public class DefaultColumnNameResolver : IColumnNameResolver
	{
		public string Resolve(string memberName) => memberName;
	}
}
