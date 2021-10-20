using System;
using System.Reflection;

namespace DefaultSqlQueryBuilder.Resolvers
{
    public class DefaultTableNameResolver : ITableNameResolver
    {
        public string Resolve(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var tableNameAttribute = type.GetCustomAttribute(typeof(TableNameAttribute), true) as TableNameAttribute;
            if(tableNameAttribute != null)
                return $"[{tableNameAttribute.TableName}]";
            
            return $"[{type.Name}]";
        }
    }
}