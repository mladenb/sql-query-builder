namespace DefaultSqlQueryBuilder.Clauses
{
	public class OrderBySqlClause : SqlClause
	{
		public string Columns { get; }

		public OrderBySqlClause(string columns)
		{
			Columns = columns;
		}
	}
}
