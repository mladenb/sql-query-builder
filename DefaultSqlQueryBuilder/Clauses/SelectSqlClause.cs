namespace DefaultSqlQueryBuilder.Clauses
{
	public class SelectSqlClause : SqlClause
	{
		public string Columns { get; }

		public SelectSqlClause(string columns)
		{
			Columns = columns;
		}
	}
}
