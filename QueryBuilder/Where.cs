using System;
using System.Linq.Expressions;

namespace QueryBuilder
{
    public class Where<T>
    {
        private string fromPart = "";
        private string clause = "";
        private string connector = "";

        private Where(string from, Expression<Func<T, object>> where, string connector = null)
        {
            fromPart = from;
            this.connector = connector ?? "WHERE";
            if (where.Body is UnaryExpression unary)
            {
                if (unary.Operand is BinaryExpression binary)
                {
                    if (binary.Left is MemberExpression left)
                    {
                        clause += left.Member.Name;
                    }
                    switch (binary.NodeType)
                    {
                        case ExpressionType.Equal:
                            clause += " = ";
                            break;
                        case ExpressionType.GreaterThan:
                            clause += " > ";
                            break;
                        case ExpressionType.GreaterThanOrEqual:
                            clause += " >= ";
                            break;
                        case ExpressionType.LessThan:
                            clause += " < ";
                            break;
                        case ExpressionType.LessThanOrEqual:
                            clause += " <= ";
                            break;
                        case ExpressionType.NotEqual:
                            clause += " != ";
                            break;
                        default:
                            clause += " ";
                            break;
                    }
                    if (binary.Right is ConstantExpression constant)
                    {
                        var value = constant.Value;
                        if(value == null)
                        {
                            clause = clause.Split(' ')[0];
                            if(binary.NodeType == ExpressionType.NotEqual)
                            {
                                clause += " IS NOT NULL";
                            }
                            else if(binary.NodeType == ExpressionType.Equal)
                            {
                                clause += " IS NULL";
                            }
                        }
                        else if (value.GetType() == typeof(string))
                        {
                            clause += $"'{value}'";
                        }
                        else
                        {
                            clause += value;
                        }
                    }
                }
            }
        }

        public Where(From<T> from, Expression<Func<T, object>> where)
            :this(from.ToString(), where)
        {
        }

        public Where<T> And(Expression<Func<T, object>> otherCondition)
        {
            return new Where<T>($"{fromPart} {connector} ({clause})", otherCondition, "AND");
        }

        public Where<T> Or(Expression<Func<T, object>> otherCondition)
        {
            return new Where<T>($"{fromPart} {connector} ({clause})", otherCondition, "OR");
        }

        public override string ToString()
        {
            return $"{fromPart} {connector} {clause}";
        }
    }
}