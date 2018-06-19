using System;

namespace DefaultSqlQueryBuilder.Resolvers
{
	public class DefaultTableNameResolver : ITableNameResolver
	{
		public string Resolve(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			return $"[{type.Name}]";
		}
	}
}
