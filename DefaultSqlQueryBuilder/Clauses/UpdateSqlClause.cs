namespace DefaultSqlQueryBuilder.Clauses
{
	public class UpdateSqlClause : SqlClause
	{
		public string TableName { get; }
		public string ColumnsWithValues { get; }
		public object[] Parameters { get; }

		public UpdateSqlClause(string tableName, string columnsWithValues, object[] parameters)
		{
			TableName = tableName;
			ColumnsWithValues = columnsWithValues;
			Parameters = parameters;
		}
	}
}
