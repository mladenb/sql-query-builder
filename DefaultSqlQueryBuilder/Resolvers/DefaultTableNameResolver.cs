using DefaultSqlQueryBuilder.Contracts;
using System;

namespace DefaultSqlQueryBuilder.Resolvers
{
	public class DefaultTableNameResolver : ITableNameResolver
	{
		public string Resolve(Type type) => type.Name;
	}
}
