using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Clauses
{
	public class OrderBySqlClause : ISqlClause
	{
		public string Columns { get; }
		public OrderingDirection OrderingDirection { get; }

		public OrderBySqlClause(string columns, OrderingDirection orderingDirection)
		{
			Columns = columns;
			OrderingDirection = orderingDirection;
		}
	}
}
