using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class LeftJoinSqlClause : ISqlClause
	{
		public string TableName { get; }
		public string OnConditions { get; }
		public object[] Parameters { get; }

		public LeftJoinSqlClause(string tableName, string onConditions, object[] parameters)
		{
			TableName = tableName;
			OnConditions = onConditions;
			Parameters = parameters;
		}
	}
}
