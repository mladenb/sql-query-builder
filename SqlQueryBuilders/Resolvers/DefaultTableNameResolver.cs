using SqlQueryBuilders.Contracts;
using System;

namespace SqlQueryBuilders.Resolvers
{
	public class DefaultTableNameResolver : ITableNameResolver
	{
		public string Resolve(Type type) => type.Name;
	}
}
