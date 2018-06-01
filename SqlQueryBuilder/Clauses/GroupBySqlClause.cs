namespace SqlQueryBuilder.Clauses
{
	public class GroupBySqlClause : SqlClause
	{
		public string Columns { get; }

		public GroupBySqlClause(string columns)
		{
			Columns = columns;
		}
	}
}
