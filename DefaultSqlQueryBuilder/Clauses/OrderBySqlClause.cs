using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class OrderBySqlClause : ISqlClause
	{
		public string Columns { get; }

		public OrderBySqlClause(string columns)
		{
			Columns = columns;
		}
	}
}
