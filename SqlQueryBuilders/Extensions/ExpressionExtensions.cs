using System.Linq.Expressions;

namespace SqlQueryBuilders.Extensions
{
	public static class ExpressionExtensions
	{
		public static MemberExpression? AsMemberExpression(this Expression? expression)
		{
			if (expression == null) return null;

			return expression as MemberExpression
				?? ((expression as UnaryExpression)?.Operand).AsMemberExpression();
		}
	}
}
