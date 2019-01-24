namespace DefaultSqlQueryBuilder.Clauses
{
	public class SelectSqlClause : SqlClause
	{
		public string Columns { get; }
		public int? Top { get; }

		public SelectSqlClause(string columns, int? top = null)
		{
			Columns = columns;
			Top = top;
		}
	}
}
