using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Brio.Docs.Common.Extensions
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo ToPropertyInfo<TSource, TDestination>(
            this Expression<Func<TSource, TDestination>> propertyExpression)
        {
            var expression = propertyExpression.Body;

            if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
                expression = unaryExpression.Operand;

            if (!(expression is MemberExpression prop && prop.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("The lambda expression must use property only", nameof(propertyExpression));

            if (!(prop.Expression is ParameterExpression))
            {
                throw new ArgumentException(
                    $"The lambda expression must use property of {typeof(TSource).Name} only",
                    nameof(propertyExpression));
            }

            return propertyInfo;
        }
    }
}
