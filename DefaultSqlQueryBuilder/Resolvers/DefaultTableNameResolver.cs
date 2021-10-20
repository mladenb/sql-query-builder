using System;

namespace DefaultSqlQueryBuilder.Resolvers
{
    public class DefaultTableNameResolver : ITableNameResolver
    {
        public string Resolve(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var customAttribute = type.GetCustomAttributes(typeof(TableNameAttribute), true);
            if (customAttribute.Length > 0)
            {
                var tableNameAttribute = customAttribute[0] as TableNameAttribute;
                return $"[{tableNameAttribute.TableName}]";
            }

            return $"[{type.Name}]";
        }
    }
}