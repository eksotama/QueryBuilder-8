using System;
using System.Linq.Expressions;

namespace QueryBuilder
{
    public class Where<T>
    {
        private From<T> from;
        private string clause = "";

        public Where(From<T> from, Expression<Func<T, object>> where)
        {
            this.from = from;
            if(where.Body is UnaryExpression unary)
            {
                if(unary.Operand is BinaryExpression binary)
                {
                    if(binary.Left is MemberExpression left)
                    {
                        clause += left.Member.Name;
                    }
                    switch(binary.NodeType)
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
                    if(binary.Right is ConstantExpression constant)
                    {
                        var value = constant.Value;
                        if(value.GetType() == typeof(string))
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

        public override string ToString()
        {
            return $"{from} WHERE {clause}";
        }
    }
}