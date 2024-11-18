using DefaultSqlQueryBuilder.Contracts;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DefaultSqlQueryBuilder.Generics
{
	public class SqlQueryBuilder<T> : SqlQueryBuilderBase
	{
		public SqlQueryBuilder(SqlQueryBuilderBase sqlQueryBuilderBase) : base(sqlQueryBuilderBase)
		{
		}

		public SqlQueryBuilder<T, TNew> LeftJoin<TNew>(Expression<Func<T, TNew, string>> stringExpression, params object[] parameters)
		{
			return UpdateAndExpand<TNew>(sqlBuilder => sqlBuilder.AddLeftJoin<TNew>(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<T, TNew> InnerJoin<TNew>(Expression<Func<T, TNew, string>> stringExpression, params object[] parameters)
		{
			return UpdateAndExpand<TNew>(sqlBuilder => sqlBuilder.AddInnerJoin<TNew>(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<T> Where(Expression<Func<T, string>> stringExpression, params object[] parameters)
		{
			return Update(sqlBuilder => sqlBuilder.AddWhere(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<T> ConditionalWhere(bool shouldFilter, Expression<Func<T, string>> stringExpression, params Func<object>[] parametersFunc)
		{
			return Update(sqlBuilder =>
			{
				if (shouldFilter)
				{
					sqlBuilder.AddWhere(ParseStringFormatExpression(stringExpression.Body), parametersFunc.Select(func => func.Invoke()).ToArray());
				}
			});
		}

		public SqlQueryBuilder<T> Select(Expression<Func<T, string>> stringExpression)
		{
			return Update(sqlBuilder => sqlBuilder.AddSelect(ParseStringFormatExpression(stringExpression.Body)));
		}

		public SqlQueryBuilder<T> GroupBy(Expression<Func<T, string>> stringExpression)
		{
			return Update(sqlBuilder => sqlBuilder.AddGroupBy(ParseStringFormatExpression(stringExpression.Body)));
		}

		public SqlQueryBuilder<T> OrderBy(Expression<Func<T, string>> stringExpression, OrderingDirection orderingDirection = OrderingDirection.Ascending)
		{
			return Update(sqlBuilder => sqlBuilder.AddOrderBy(ParseStringFormatExpression(stringExpression.Body), orderingDirection));
		}

		public SqlQueryBuilder<T> Skip(int rowCount)
		{
			return Update(sqlBuilder => sqlBuilder.AddSkip(rowCount));
		}

		public SqlQueryBuilder<T> Take(int rowCount)
		{
			return Update(sqlBuilder => sqlBuilder.AddTake(rowCount));
		}

		public SqlQueryBuilder<T> Custom(Expression<Func<T, string>> stringExpression, params object[] parameters)
		{
			return Update(sqlBuilder => sqlBuilder.AddCustom(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		/// <summary>
		/// Instead of simply updating our object, we keep it immutable and we create and update the clone instead.
		/// </summary>
		/// <param name="updateAction">The action which updates the current object</param>
		/// <returns>A clone of the current object, updated using the <paramref name="updateAction"/></returns>
		private SqlQueryBuilder<T> Update(Action<SqlQueryBuilder<T>> updateAction)
		{
			var clone = Clone();
			updateAction(clone);

			return clone;
		}

		/// <summary>
		/// Similar to <see cref="Update"/> method, but it returns the builder with 1 additional generic parameter <see cref="TNew"/>.
		/// </summary>
		/// <param name="updateAction">The action which updates the current object</param>
		/// <returns>A clone of the current object (with additional generic parameter <see cref="TNew"/>), updated using the <paramref name="updateAction"/></returns>
		/// <seealso cref="Update"/>
		private SqlQueryBuilder<T, TNew> UpdateAndExpand<TNew>(Action<SqlQueryBuilder<T>> updateAction)
		{
			var clone = Update(updateAction);

			return new SqlQueryBuilder<T, TNew>(clone);
		}

		private SqlQueryBuilder<T> Clone()
		{
			return new SqlQueryBuilder<T>(this);
		}
	}
}
