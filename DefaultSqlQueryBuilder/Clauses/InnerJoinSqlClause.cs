using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class InnerJoinSqlClause : ISqlClause
	{
		public string TableName { get; }
		public string OnConditions { get; }
		public object[] Parameters { get; }

		public InnerJoinSqlClause(string tableName, string onConditions, object[] parameters)
		{
			TableName = tableName;
			OnConditions = onConditions;
			Parameters = parameters;
		}
	}
}
