using SqlQueryBuilders.Contracts;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilders.Generics
{
	public class SqlQueryBuilder<T1, T2, T3> : SqlQueryBuilderBase
	{
		public SqlQueryBuilder(SqlQueryBuilderBase sqlQueryBuilderBase) : base(sqlQueryBuilderBase)
		{
		}

		public SqlQueryBuilder<T1, T2, T3, TNew> LeftJoin<TNew>(Expression<Func<T1, T2, T3, TNew, string>> stringExpression, params object[] parameters)
		{
			return UpdateAndExpand<TNew>(sqlBuilder => sqlBuilder.AddLeftJoin<TNew>(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<T1, T2, T3, TNew> InnerJoin<TNew>(Expression<Func<T1, T2, T3, TNew, string>> stringExpression, params object[] parameters)
		{
			return UpdateAndExpand<TNew>(sqlBuilder => sqlBuilder.AddInnerJoin<TNew>(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<T1, T2, T3> Where(Expression<Func<T1, T2, T3, string>> stringExpression, params object[] parameters)
		{
			return Update(sqlBuilder => sqlBuilder.AddWhere(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<T1, T2, T3> ConditionalWhere(bool shouldFilter, Expression<Func<T1, T2, T3, string>> stringExpression, params Func<object>[] parametersFunc)
		{
			return Update(sqlBuilder =>
			{
				if (shouldFilter)
				{
					sqlBuilder.AddWhere(ParseStringFormatExpression(stringExpression.Body), parametersFunc.Select(func => func.Invoke()).ToArray());
				}
			});
		}

		public SqlQueryBuilder<T1, T2, T3> Select(Expression<Func<T1, T2, T3, string>> stringExpression)
		{
			return Update(sqlBuilder => sqlBuilder.AddSelect(ParseStringFormatExpression(stringExpression.Body)));
		}

		public SqlQueryBuilder<T1, T2, T3> GroupBy(Expression<Func<T1, T2, T3, string>> stringExpression)
		{
			return Update(sqlBuilder => sqlBuilder.AddGroupBy(ParseStringFormatExpression(stringExpression.Body)));
		}

		public SqlQueryBuilder<T1, T2, T3> OrderBy(Expression<Func<T1, T2, T3, string>> stringExpression, OrderingDirection orderingDirection = OrderingDirection.Ascending)
		{
			return Update(sqlBuilder => sqlBuilder.AddOrderBy(ParseStringFormatExpression(stringExpression.Body), orderingDirection));
		}

		public SqlQueryBuilder<T1, T2, T3> Skip(int rowCount)
		{
			return Update(sqlBuilder => sqlBuilder.AddSkip(rowCount));
		}

		public SqlQueryBuilder<T1, T2, T3> Take(int rowCount)
		{
			return Update(sqlBuilder => sqlBuilder.AddTake(rowCount));
		}

		public SqlQueryBuilder<T1, T2, T3> Custom(Expression<Func<T1, T2, T3, string>> stringExpression, params object[] parameters)
		{
			return Update(sqlBuilder => sqlBuilder.AddCustom(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		/// <summary>
		/// Instead of simply updating our object, we keep it immutable and we create and update the clone instead.
		/// </summary>
		/// <param name="updateAction">The action which updates the current object</param>
		/// <returns>A clone of the current object, updated using the <paramref name="updateAction"/></returns>
		private SqlQueryBuilder<T1, T2, T3> Update(Action<SqlQueryBuilder<T1, T2, T3>> updateAction)
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
		private SqlQueryBuilder<T1, T2, T3, TNew> UpdateAndExpand<TNew>(Action<SqlQueryBuilder<T1, T2, T3>> updateAction)
		{
			var clone = Update(updateAction);

			return new SqlQueryBuilder<T1, T2, T3, TNew>(clone);
		}

		private SqlQueryBuilder<T1, T2, T3> Clone()
		{
			return new SqlQueryBuilder<T1, T2, T3>(this);
		}
	}
}
