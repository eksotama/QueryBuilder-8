using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ArLehm.QueryBuilder
{
    public class Where<T>
    {
        private string fromPart = "";
        private string clause = "";
        private string connector = "";

        Type valueType;

        public Where(Expression<Func<T, object>> where)
            :this("", where, "")
        { }

        private Where(string from, Expression<Func<T, object>> where, string connector = null)
        {
            fromPart = from;
            this.connector = connector ?? "WHERE";
            if(where.Body is MemberExpression memberEx)
            {
                clause += memberEx.Member.Name;
                valueType = (memberEx.Member as PropertyInfo).PropertyType;
            }
            else if (where.Body is UnaryExpression unary)
            {
                if(unary.Operand is MemberExpression member)
                {
                    clause += member.Member.Name;
                    valueType = (member.Member as PropertyInfo).PropertyType;
                }
                else if (unary.Operand is BinaryExpression binary)
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
                        if (value == null)
                        {
                            clause = clause.Split(' ')[0];
                            if (binary.NodeType == ExpressionType.NotEqual)
                            {
                                clause += " IS NOT NULL";
                            }
                            else if (binary.NodeType == ExpressionType.Equal)
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
                    else if(binary.Right is MemberExpression memberRight)
                    {
                        if(memberRight.Expression is ConstantExpression rightConstant)
                        {
                            var val = ((FieldInfo)memberRight.Member).GetValue(rightConstant.Value);
                            if (val.GetType() == typeof(string))
                            {
                                clause += $"'{val}'";
                            }
                            else
                            {
                                clause += $"{val}";
                            }
                        }
                    }
                }
            }
        }

        public Where(From<T> from, Expression<Func<T, object>> where)
            : this(from.ToString(), where)
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

        public Where<T> In<TValue>(IEnumerable<TValue> values)
        {
            if (typeof(TValue) == typeof(string))
            {
                clause += $" IN ({string.Join(",", values.Select(v => $"'{v}'"))})";
            }
            else
            {
                clause += $" IN ({string.Join(",", values)})";
            }
            return this;
        }

        public Where<T> NotIn<TValue>(IEnumerable<TValue> values)
        {
            if (typeof(TValue) == typeof(string))
            {
                clause += $" NOT IN ({string.Join(",", values.Select(v => $"'{v}'"))})";
            }
            else
            {
                clause += $" NOT IN ({string.Join(",", values)})";
            }
            return this;
        }

        public Where<T> Like(string like)
        {
            if(valueType != typeof(string) && valueType != typeof(char))
            {
                throw new ArgumentException("like can only be used for strings!");
            }
            clause += $" LIKE '{like}'";
            return this;
        }

        public OrderBy<T> OrderBy(Expression<Func<T, object>> orderBy)
        {
            return new OrderBy<T>(this, orderBy);
        }

        public override string ToString()
        {
            return $"{fromPart} {connector} {clause}".Trim();
        }
    }
}