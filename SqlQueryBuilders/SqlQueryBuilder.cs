using SqlQueryBuilders.Contracts;
using SqlQueryBuilders.Generics;
using SqlQueryBuilders.Resolvers;
using System;
using System.Linq.Expressions;

namespace SqlQueryBuilders
{
	public class SqlQueryBuilder : SqlQueryBuilderBase
	{
		public SqlQueryBuilder(ISqlSyntax sqlSyntax)
			: this(sqlSyntax, new DefaultTableNameResolver(), new DefaultColumnNameResolver())
		{
		}

		public SqlQueryBuilder(ISqlSyntax sqlSyntax, ITableNameResolver tableNameResolver, IColumnNameResolver columnNameResolver)
			: base(sqlSyntax, tableNameResolver, columnNameResolver)
		{
		}

		private SqlQueryBuilder(SqlQueryBuilderBase sqlQueryBuilderBase) : base(sqlQueryBuilderBase)
		{
		}

		public SqlQueryBuilder CreateTableIfNotExists<TTable>()
		{
			var clone = Clone();
			clone.AddCreateTableIfNotExists<TTable>();

			return clone;
		}

		public SqlQueryBuilder<TTable> From<TTable>()
		{
			return UpdateAndExpand<TTable>(sqlBuilder => sqlBuilder.AddFrom<TTable>());
		}

		public SqlQueryBuilder<TTable> Insert<TTable>(Expression<Func<TTable, string>> stringExpression, params object[] parameters)
		{
			return UpdateAndExpand<TTable>(sqlBuilder => sqlBuilder.AddInsert<TTable>(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<TTable> InsertMultiple<TTable>(Expression<Func<TTable, string>> stringExpression, params object[][] parameters)
		{
			return UpdateAndExpand<TTable>(sqlBuilder => sqlBuilder.AddInsertMultiple<TTable>(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<TTable> Update<TTable>(Expression<Func<TTable, string>> stringExpression, params object[] parameters)
		{
			return UpdateAndExpand<TTable>(sqlBuilder => sqlBuilder.AddUpdate<TTable>(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		public SqlQueryBuilder<TTable> Delete<TTable>()
		{
			return UpdateAndExpand<TTable>(sqlBuilder => sqlBuilder.AddDelete<TTable>());
		}

		public SqlQueryBuilder<TTable> Custom<TTable>(Expression<Func<TTable, string>> stringExpression, params object[] parameters)
		{
			return UpdateAndExpand<TTable>(sqlBuilder => sqlBuilder.AddFirstCustom(ParseStringFormatExpression(stringExpression.Body), parameters));
		}

		private SqlQueryBuilder<TNew> UpdateAndExpand<TNew>(Action<SqlQueryBuilder> updateAction)
		{
			var clone = Clone();
			updateAction(clone);

			return new SqlQueryBuilder<TNew>(clone);
		}

		private SqlQueryBuilder Clone()
		{
			return new SqlQueryBuilder(this);
		}
	}
}
