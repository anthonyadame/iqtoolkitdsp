
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQToolkitDSP
{
    internal static class TypeSystem
    {

        /// <summary>
        /// Gets the type of the I enumerable element.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetIEnumerableElementType(Type type)
        {
            Type ienum = FindIEnumerable(type);
            if (ienum == null)
            {
                return null;
            }

            return ienum.GetGenericArguments()[0];
        }

        /// <summary>
        /// Finds the I enumerable.
        /// </summary>
        /// <param name="seqType">Type of the seq.</param>
        /// <returns></returns>
        public static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }

            return null;
        }

        /// <summary>
        /// Finds the I queryable.
        /// </summary>
        /// <param name="seqType">Type of the seq.</param>
        /// <returns></returns>
        public static Type FindIQueryable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IQueryable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIQueryable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIQueryable(seqType.BaseType);
            }

            return null;
        }

        /// <summary>
        /// Gets the type of the nullable underlying.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetNullableUnderlyingType(Type type)
        {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                return null;
            }

            return type.GetGenericArguments()[0];
        }

        /// <summary>
        /// Determines whether [is linq named method second argument function with one parameter] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>
        ///   <c>true</c> if [is linq named method second argument function with one parameter] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLinqNamedMethodSecondArgumentFunctionWithOneParameter(MethodInfo m, string methodName)
        {
            if (m.DeclaringType == typeof(Enumerable))
            {
                return IsNamedMethodSecondArgumentFuncWithOneParameter(m, methodName);
            }
            else
            {
                return IsNamedMethodSecondArgumentExpressionFuncWithOneParameter(m, methodName);
            }
        }

        /// <summary>
        /// Determines whether [is method linq select] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns>
        ///   <c>true</c> if [is method linq select] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMethodLinqSelect(MethodInfo m)
        {
            return IsLinqNamedMethodSecondArgumentFunctionWithOneParameter(m, "Select");
        }

        /// <summary>
        /// Determines whether [is method linq select many] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns>
        ///   <c>true</c> if [is method linq select many] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMethodLinqSelectMany(MethodInfo m)
        {
            return IsLinqNamedMethodSecondArgumentFunctionWithOneParameter(m, "SelectMany");
        }

        /// <summary>
        /// Determines whether [is method linq where] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns>
        ///   <c>true</c> if [is method linq where] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMethodLinqWhere(MethodInfo m)
        {
            return IsLinqNamedMethodSecondArgumentFunctionWithOneParameter(m, "Where");
        }

        /// <summary>
        /// Determines whether [is method linq order by] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns>
        ///   <c>true</c> if [is method linq order by] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMethodLinqOrderBy(MethodInfo m)
        {
            return IsLinqNamedMethodSecondArgumentFunctionWithOneParameter(m, "OrderBy");
        }

        /// <summary>
        /// Determines whether [is method linq then by] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns>
        ///   <c>true</c> if [is method linq then by] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMethodLinqThenBy(MethodInfo m)
        {
            return IsLinqNamedMethodSecondArgumentFunctionWithOneParameter(m, "ThenBy");
        }

        /// <summary>
        /// Determines whether [is named method second argument expression func with one parameter] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if [is named method second argument expression func with one parameter] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNamedMethodSecondArgumentExpressionFuncWithOneParameter(MethodInfo m, string name)
        {
            Debug.Assert(m != null, "m != null");
            Debug.Assert(!String.IsNullOrEmpty(name), "!String.IsNullOrEmpty(name)");
            if (m.Name == name)
            {
                ParameterInfo[] p = m.GetParameters();
                if (p != null &&
                    p.Length == 2 &&
                    p[0].ParameterType.IsGenericType &&
                    p[1].ParameterType.IsGenericType)
                {
                    Type expressionParameter = p[1].ParameterType;
                    Type[] genericArgs = expressionParameter.GetGenericArguments();
                    if (genericArgs.Length == 1 && expressionParameter.GetGenericTypeDefinition() == typeof(Expression<>))
                    {
                        Type functionParameter = genericArgs[0];
                        return functionParameter.IsGenericType && functionParameter.GetGenericTypeDefinition() == typeof(Func<,>);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is named method second argument func with one parameter] [the specified m].
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if [is named method second argument func with one parameter] [the specified m]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNamedMethodSecondArgumentFuncWithOneParameter(MethodInfo m, string name)
        {
            Debug.Assert(m != null, "m != null");
            Debug.Assert(!String.IsNullOrEmpty(name), "!String.IsNullOrEmpty(name)");
            if (m.Name == name)
            {
                ParameterInfo[] p = m.GetParameters();
                if (p != null &&
                    p.Length == 2 &&
                    p[0].ParameterType.IsGenericType &&
                    p[1].ParameterType.IsGenericType)
                {
                    Type functionParameter = p[1].ParameterType;
                    return functionParameter.IsGenericType && functionParameter.GetGenericTypeDefinition() == typeof(Func<,>);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the type of the property info for.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="setter">if set to <c>true</c> [setter].</param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfoForType(Type t, string propertyName, bool setter)
        {
            PropertyInfo pi = null;

            try
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                flags |= setter ? BindingFlags.SetProperty : BindingFlags.GetProperty;

                pi = t.GetProperty(propertyName, flags);

                if (pi == null)
                {
                    throw new NotSupportedException(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Failed to find property {0} on type {1}", propertyName, t.Name));
                }
            }
            catch (Exception exception)
            {
                throw new NotSupportedException(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Error finding property {0}", propertyName), exception);
            }

            return pi;
        }
    
    }
}