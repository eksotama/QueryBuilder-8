using System;
using System.Linq.Expressions;

namespace ArLehm.QueryBuilder
{
    public class OrderBy<T>
    {
        private Where<T> wherePart;
        private string orderPart;
        private string orderDirection;

        public OrderBy(Where<T> where, Expression<Func<T, object>> orderBy, string direction = "asc")
        {
            wherePart = where;
            orderDirection = direction;
            orderPart = "";

            if(orderBy.Body is MemberExpression member)
            {
                orderPart += member.Member.Name;
            }
            else if(orderBy.Body is UnaryExpression unary)
            {
                if(unary.Operand is MemberExpression unaryMember)
                {
                    orderPart += unaryMember.Member.Name;
                }
            }
        }

        public override string ToString()
        {
            return $"{wherePart} ORDER BY {orderPart}";
        }
    }
}