using DefaultSqlQueryBuilder.Clauses;
using DefaultSqlQueryBuilder.Contracts;
using DefaultSqlQueryBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DefaultSqlQueryBuilder
{
	public abstract class SqlQueryBuilderBase
	{
		protected SqlQueryBuilderBase(ITableNameResolver tableNameResolver, IColumnNameResolver columnNameResolver, IEnumerable<SqlClause> clauses = null)
		{
			TableNameResolver = tableNameResolver;
			ColumnNameResolver = columnNameResolver;
			_clauses = clauses?.ToList() ?? new List<SqlClause>();
		}

		protected SqlQueryBuilderBase(SqlQueryBuilderBase sqlQueryBuilderBase)
			: this(sqlQueryBuilderBase.TableNameResolver, sqlQueryBuilderBase.ColumnNameResolver, sqlQueryBuilderBase._clauses)
		{
		}

		public ITableNameResolver TableNameResolver { get; }
		public IColumnNameResolver ColumnNameResolver { get; }
		public IReadOnlyCollection<SqlClause> Clauses => _clauses.ToArray();

		private readonly List<SqlClause> _clauses;

		public SqlQuery ToSqlQuery(ISqlSyntax syntax)
		{
			var sqls = _clauses
				.ConsolidateWhereClauses()
				.Select(c => syntax.ToSql(c));

			var result = sqls.Aggregate((current, sql) => current.Append(sql.Sql, sql.Parameters));

			return new SqlQuery(result.Sql, result.Parameters);
		}

		protected string TableNameFor<T>()
		{
			return TableNameFor(typeof(T));
		}

		protected string TableNameFor(Type type)
		{
			return TableNameResolver.Resolve(type);
		}

		protected string ColumnNameFor(Expression expression)
		{
			var memberExpression = expression.AsMemberExpression();
			if (memberExpression == null) return null;

			return ColumnNameFor(memberExpression.Expression.Type, memberExpression.Member.Name);
		}

		protected string ColumnNameOrTableNameFor(Expression expression)
		{
			return ColumnNameFor(expression) ?? TableNameFor(expression.Type);
		}

		protected void AddLeftJoin<TTable>(string onConditions, object[] parameters)
		{
			_clauses.Add(new LeftJoinSqlClause(TableNameFor<TTable>(), onConditions, parameters));
		}

		protected void AddInnerJoin<TTable>(string onConditions, object[] parameters)
		{
			_clauses.Add(new InnerJoinSqlClause(TableNameFor<TTable>(), onConditions, parameters));
		}

		protected void AddWhere(string whereConditions, object[] parameters)
		{
			_clauses.Add(new WhereSqlClause(whereConditions, parameters));
		}

		protected void AddFrom<TTable>()
		{
			_clauses.Add(new FromSqlClause(TableNameFor<TTable>()));
		}

		protected void AddSelect(string columns)
		{
			_clauses.RemoveAll(clause => clause is SelectSqlClause);
			_clauses.Insert(0, new SelectSqlClause(columns));
		}

		protected void AddInsert<TTable>(string columns, object[] parameters)
		{
			_clauses.RemoveAll(clause => clause is InsertSqlClause);
			_clauses.RemoveAll(clause => clause is InsertMultipleSqlClause);
			_clauses.Insert(0, new InsertSqlClause(TableNameFor<TTable>(), columns, parameters));
		}

		protected void AddInsertMultiple<TTable>(string columns, object[][] parameters)
		{
			_clauses.RemoveAll(clause => clause is InsertSqlClause);
			_clauses.RemoveAll(clause => clause is InsertMultipleSqlClause);
			_clauses.Insert(0, new InsertMultipleSqlClause(TableNameFor<TTable>(), columns, parameters));
		}

		protected void AddUpdate<TTable>(string columns, object[] parameters)
		{
			_clauses.RemoveAll(clause => clause is UpdateSqlClause);
			_clauses.Insert(0, new UpdateSqlClause(TableNameFor<TTable>(), columns, parameters));
		}

		protected void AddDelete<TTable>()
		{
			_clauses.Add(new DeleteSqlClause(TableNameFor<TTable>()));
		}

		protected void AddFirstCustom(string sql, object[] parameters)
		{
			_clauses.Insert(0, new CustomSqlClause(sql, parameters));
		}

		protected void AddGroupBy(string columns)
		{
			_clauses.Add(new GroupBySqlClause(columns));
		}

		protected void AddOrderBy(string columns)
		{
			_clauses.Add(new OrderBySqlClause(columns));
		}

		protected void AddCustom(string sql, object[] parameters)
		{
			_clauses.Add(new CustomSqlClause(sql, parameters));
		}

		protected string ParseStringFormatExpression(Expression expression)
		{
			var simpleString = GetSimpleString(expression);
			if (simpleString != null) return simpleString;

			var stringFormatExpression = GetStringFormatMethodCallExpression(expression);

			var stringPattern = ((ConstantExpression)stringFormatExpression.Arguments.First())
				.Value
				.ToString();

			var mappedArguments = stringFormatExpression
				.Arguments
				.Skip(1)
				.SelectMany(ToExpressionList)
				.Select(ColumnNameOrTableNameFor);

			return string.Format(stringPattern, mappedArguments.Cast<object>().ToArray());
		}

		private static string GetSimpleString(Expression expression)
		{
			if (expression.Type != typeof(string)) return null;

			if (expression.NodeType == ExpressionType.Constant) return (string)((ConstantExpression)expression).Value;

			if (expression.NodeType == ExpressionType.Coalesce)
			{
				var binaryExpression = (BinaryExpression)expression;
				var coalesced = Expression.Coalesce(binaryExpression.Left, binaryExpression.Right);
				var lambda = Expression.Lambda<Func<string>>(coalesced);
				var compiled = lambda.Compile();

				return compiled.Invoke();
			}

			return null;
		}

		private static List<Expression> ToExpressionList(Expression x)
		{
			return x.NodeType == ExpressionType.NewArrayInit
				? ((NewArrayExpression)x).Expressions.ToList()
				: new List<Expression> { x };
		}

		private MethodCallExpression GetStringFormatMethodCallExpression(Expression expression)
		{
			return expression is MethodCallExpression mce && IsStringFormatMethodCallExpression(mce)
				? mce
				: throw new ArgumentException("Expression is not a String.Format() MethodCallExpression", nameof(expression));
		}

		private bool IsStringFormatMethodCallExpression(MethodCallExpression mce)
		{
			return mce.Method.DeclaringType == typeof(string)
				&& mce.Method.Name == nameof(string.Format)
				&& mce.Arguments.First() is ConstantExpression;
		}

		private string ColumnNameFor(Type type, string memberName)
		{
			return ColumnNameResolver.Resolve(type, memberName);
		}
	}
}
