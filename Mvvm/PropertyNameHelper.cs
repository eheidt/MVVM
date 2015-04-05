using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MVVM
{
    public static class PropertyNameHelper 
    {
        public static string ExtractName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression != null)
            {
                var property = memberExpression.Member as PropertyInfo;
                if (property != null)
                {
                    var getMethod = property.GetGetMethod(true);
                    if (getMethod.IsStatic == false)
                        return memberExpression.Member.Name;
                }
            }

            throw new InvalidOperationException();
        }
    }
}
