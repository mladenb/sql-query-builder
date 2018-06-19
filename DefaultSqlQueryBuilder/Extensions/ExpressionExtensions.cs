using System;
using System.Linq.Expressions;

namespace DefaultSqlQueryBuilder.Extensions
{
	public static class ExpressionExtensions
	{
		public static MemberExpression AsMemberExpression(this Expression expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			return expression as MemberExpression
				?? AsMemberExpression((expression as UnaryExpression)?.Operand);
		}
	}
}
