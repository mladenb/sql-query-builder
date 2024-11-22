using SqlQueryBuilders.Clauses;
using SqlQueryBuilders.Contracts;
using SqlQueryBuilders.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilders
{
	public class SqlQueryBuilderBase
	{
		protected SqlQueryBuilderBase(ISqlSyntax sqlSyntax, ITableNameResolver tableNameResolver, IColumnNameResolver columnNameResolver)
		{
			TableNameResolver = tableNameResolver;
			ColumnNameResolver = columnNameResolver;
			SqlSyntax = sqlSyntax;
		}

		protected SqlQueryBuilderBase(SqlQueryBuilderBase sqlQueryBuilderBase)
			: this(sqlQueryBuilderBase.SqlSyntax, sqlQueryBuilderBase.TableNameResolver, sqlQueryBuilderBase.ColumnNameResolver)
		{
			_clauses = sqlQueryBuilderBase._clauses;
		}

		public ITableNameResolver TableNameResolver { get; }
		public IColumnNameResolver ColumnNameResolver { get; }
		public ISqlSyntax SqlSyntax { get; }

		public IReadOnlyCollection<ISqlClause> Clauses => _clauses.ToArray();

		private readonly List<ISqlClause> _clauses = new List<ISqlClause>();

		public SqlQuery ToSqlQuery()
		{
			var consolidatedClauses = _clauses.ConsolidateWhereClauses();
			var sqls = SqlSyntax.ToSqlQuery(consolidatedClauses);

			var result = sqls.Aggregate((current, sql) => current.Append(sql.Sql, sql.Parameters));

			return new SqlQuery(result.Sql, result.Parameters);
		}

		protected string TableNameEscapedFor<T>() => TableNameEscapedFor(typeof(T));

		protected string TableNameEscapedFor(Type type) => SqlSyntax.EscapeTableName(TableNameResolver.Resolve(type));

		protected string? ColumnNameEscapedFor(Expression expression)
		{
			var memberExpression = expression.AsMemberExpression();
			if (memberExpression == null) return null;

			return ColumnNameEscapedFor(memberExpression.Expression.Type, memberExpression.Member.Name);
		}

		private string ColumnNameEscapedFor(Type type, string memberName)
		{
			return SqlSyntax.EscapeTableName(TableNameResolver.Resolve(type))
				+ "."
				+ SqlSyntax.EscapeColumnName(ColumnNameResolver.Resolve(memberName));
		}

		protected string ColumnNameOrTableNameEscapedFor(Expression expression)
		{
			return ColumnNameEscapedFor(expression) ?? TableNameEscapedFor(expression.Type);
		}

		protected void AddLeftJoin<TTable>(string onConditions, object[] parameters)
		{
			_clauses.Add(new LeftJoinSqlClause(TableNameEscapedFor<TTable>(), onConditions, parameters));
		}

		protected void AddInnerJoin<TTable>(string onConditions, object[] parameters)
		{
			_clauses.Add(new InnerJoinSqlClause(TableNameEscapedFor<TTable>(), onConditions, parameters));
		}

		protected void AddWhere(string whereConditions, object[] parameters)
		{
			_clauses.Add(new WhereSqlClause(whereConditions, parameters));
		}

		protected void AddFrom<TTable>()
		{
			_clauses.Add(new FromSqlClause(TableNameEscapedFor<TTable>()));
		}

		protected void AddSelect(string columns)
		{
			_clauses.RemoveAll(clause => clause is SelectSqlClause);
			_clauses.Insert(0, new SelectSqlClause(columns));
		}

		protected void AddCreateTableIfNotExists<TTable>()
		{
			var tableType = typeof(TTable);
			var tableName = TableNameResolver.Resolve(tableType);
			var columns = tableType
				.GetProperties()
				.ToDictionary(p => ColumnNameResolver.Resolve(p.Name), p => p);

			_clauses.Clear();
			_clauses.Insert(0, new CreateTableIfNotExistsClause(tableName, columns));
		}

		protected void AddInsert<TTable>(string columns, object[] parameters)
		{
			_clauses.RemoveAll(clause => clause is InsertSqlClause);
			_clauses.RemoveAll(clause => clause is InsertMultipleSqlClause);
			_clauses.Insert(0, new InsertSqlClause(TableNameEscapedFor<TTable>(), columns, parameters));
		}

		protected void AddInsertMultiple<TTable>(string columns, object[][] parameters)
		{
			_clauses.RemoveAll(clause => clause is InsertSqlClause);
			_clauses.RemoveAll(clause => clause is InsertMultipleSqlClause);
			_clauses.Insert(0, new InsertMultipleSqlClause(TableNameEscapedFor<TTable>(), columns, parameters));
		}

		protected void AddUpdate<TTable>(string columns, object[] parameters)
		{
			_clauses.RemoveAll(clause => clause is UpdateSqlClause);
			_clauses.Insert(0, new UpdateSqlClause(TableNameEscapedFor<TTable>(), columns, parameters));
		}

		protected void AddDelete<TTable>()
		{
			_clauses.Add(new DeleteSqlClause(TableNameEscapedFor<TTable>()));
		}

		protected void AddFirstCustom(string sql, object[] parameters)
		{
			_clauses.Insert(0, new CustomSqlClause(sql, parameters));
		}

		protected void AddGroupBy(string columns)
		{
			_clauses.Add(new GroupBySqlClause(columns));
		}

		protected void AddOrderBy(string columns, OrderingDirection orderingDirection)
		{
			_clauses.Add(new OrderBySqlClause(columns, orderingDirection));
		}

		protected void AddSkip(int rowCount)
		{
			_clauses.Add(new SkipSqlClause(rowCount));
		}

		protected void AddTake(int rowCount)
		{
			_clauses.Add(new TakeSqlClause(rowCount));
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
				.Select(ColumnNameOrTableNameEscapedFor);

			return string.Format(stringPattern, mappedArguments.Cast<object>().ToArray());
		}

		private static string? GetSimpleString(Expression expression)
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
	}
}
