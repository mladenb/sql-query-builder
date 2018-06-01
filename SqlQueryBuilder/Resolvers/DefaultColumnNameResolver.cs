using System;

namespace SqlQueryBuilder.Resolvers
{
	public class DefaultColumnNameResolver : IColumnNameResolver
	{
		public string Resolve(Type type, string memberName)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (memberName == null) throw new ArgumentNullException(nameof(memberName));

			return $"[{type.Name}].[{memberName}]";
		}
	}
}
