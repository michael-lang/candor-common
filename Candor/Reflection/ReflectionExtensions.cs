using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Candor.Reflection
{
	/// <summary>
	/// Defines some extension methods for enhanced reflection based operations.
	/// </summary>
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Gets the property info as a result of the property at the end of a lambda expression.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="source">The source object instance.</param>
		/// <param name="propertyLambda">The lamda expression.</param>
		/// <returns></returns>
		public static PropertyInfo PropertyInfo<TSource, TProperty>( this TSource source, System.Linq.Expressions.Expression<Func<TSource, TProperty>> propertyLambda )
		{
			Type type = typeof(TSource);

			MemberExpression member = propertyLambda.Body as MemberExpression;
			if (member == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					propertyLambda.ToString()));

			PropertyInfo propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a field, not a property.",
					propertyLambda.ToString()));

			return propInfo;
		}
		/// <summary>
		/// Gets the attribute instance of a specific type from a property specified by a lambda expression, if applicable.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="source">The source instance.</param>
		/// <param name="propertyLambda">A property lambda expression.</param>
		/// <returns>The attribute instance.</returns>
		public static TAttribute GetAttribute<TSource, TProperty, TAttribute>( this TSource source, System.Linq.Expressions.Expression<Func<TSource, TProperty>> propertyLambda )
			where TSource : class
		{
			PropertyInfo propInfo = source.PropertyInfo(propertyLambda);

			TAttribute attr = (TAttribute)propInfo.GetCustomAttributes(typeof(TAttribute), true).SingleOrDefault();
			if (attr != null)
				return attr;

			return default(TAttribute);
		}
		/// <summary>
		/// Gets the attribute instance of a runtime MemberInfo (and thus PropertyInfo) instance.
		/// </summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="propInfo"></param>
		/// <returns></returns>
		public static TAttribute GetAttribute<TAttribute>( this MemberInfo propInfo )
		{
			return (TAttribute)propInfo.GetCustomAttributes(typeof(TAttribute), true).SingleOrDefault();
		}
		/// <summary>
		/// Gets the display name of a property given a lambda expression.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="source">The source instance.</param>
		/// <param name="propertyLambda">A property lambda expression.</param>
		/// <returns></returns>
		public static String DisplayName<TSource, TProperty>( this TSource source, System.Linq.Expressions.Expression<Func<TSource, TProperty>> propertyLambda )
			where TSource : class
		{
			PropertyInfo propInfo = source.PropertyInfo(propertyLambda);
			return propInfo.GetDisplayName();
		}
		/// <summary>
		/// Gets the display name of a runtime MemberInfo (and thus PropertyInfo) instance.
		/// </summary>
		/// <param name="propInfo"></param>
		/// <returns></returns>
		public static String GetDisplayName( this MemberInfo propInfo )
		{
			DisplayNameAttribute attr = propInfo.GetAttribute<DisplayNameAttribute>();
			if (attr != null && !string.IsNullOrEmpty(attr.DisplayName))
				return attr.DisplayName;

			DisplayAttribute attr2 = propInfo.GetAttribute<DisplayAttribute>();
			if (attr2 != null && !string.IsNullOrEmpty(attr2.Name))
				return attr2.Name;
			if (attr2 != null && !string.IsNullOrEmpty(attr2.ShortName))
				return attr2.ShortName;

			return propInfo.Name;
		}
		/// <summary>
		/// Gets the description of a property given a lambda expression.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="source">The source instance.</param>
		/// <param name="propertyLambda">A property lambda expression.</param>
		/// <returns></returns>
		public static String Description<TSource, TProperty>( this TSource source, System.Linq.Expressions.Expression<Func<TSource, TProperty>> propertyLambda )
			where TSource : class
		{
			PropertyInfo propInfo = source.PropertyInfo(propertyLambda);
			return propInfo.GetDescription();
		}
		/// <summary>
		/// Gets the description of an enum value.
		/// </summary>
		/// <param name="this">The enum value.</param>
		/// <returns>The value of the Description attribute, or if missing then the enum value.</returns>
		public static string Description( this Enum @this )
		{
			string stringValue = @this.ToString();

			FieldInfo fieldInfo = @this.GetType().GetField(stringValue);

			var descriptionAttribute = (DescriptionAttribute)
				Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));

			return (descriptionAttribute == null)
				? stringValue
				: descriptionAttribute.Description;
		}
		/// <summary>
		/// Gets the description of a runtime MemberInfo (and thus PropertyInfo) instance.
		/// </summary>
		/// <param name="propInfo"></param>
		/// <returns></returns>
		public static String GetDescription( this MemberInfo propInfo )
		{
			DescriptionAttribute attr = propInfo.GetAttribute<DescriptionAttribute>();
			if (attr != null && !string.IsNullOrEmpty(attr.Description))
				return attr.Description;
			DisplayAttribute attr2 = propInfo.GetAttribute<DisplayAttribute>();
			if (attr2 != null && !string.IsNullOrEmpty(attr2.Description))
				return attr2.Description;

			return string.Empty;
		}
		/// <summary>
		/// Gets a full property name with a specified separator between the parts of the expression chain.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="exp">A lamda expression.  Example: "model=> model.Contact.Name"</param>
		/// <param name="separator">The separator, such as "." or "_"</param>
		/// <returns>"Contact.Name", or "Contact_Name"</returns>
		public static string GetFullPropertyName<T, TProperty>( this System.Linq.Expressions.Expression<Func<T, TProperty>> exp, string separator )
		{
			MemberExpression memberExp;
			if (!exp.Body.TryFindMemberExpression(out memberExp))
				return string.Empty;

			var memberNames = new Stack<string>();
			do
			{
				memberNames.Push(memberExp.Member.Name);
			}
			while (memberExp.Expression.TryFindMemberExpression(out memberExp));

			return string.Join(separator, memberNames.ToArray());
		}
		/// <summary>
		/// Finds the member expression portion of a lamda expression, stripping out any conversions/casts.
		/// </summary>
		/// <param name="exp">A lamda expression.</param>
		/// <param name="memberExp">An output of the simple expression</param>
		/// <returns>True if a member expression was found, otherwise false.</returns>
		public static bool TryFindMemberExpression( this System.Linq.Expressions.Expression exp, out MemberExpression memberExp )
		{
			memberExp = exp as MemberExpression;
			if (memberExp != null)
				return true;

			if (exp.IsConversion() && exp is UnaryExpression)
			{
				memberExp = ((UnaryExpression)exp).Operand as MemberExpression;
				if (memberExp != null)
				{
					return true;
				}
			}

			return false;
		}
		/// <summary>
		/// Determines if this is a conversion expression.
		/// </summary>
		/// <param name="exp"></param>
		/// <returns></returns>
		/// <remarks>
		/// if the compiler created an automatic conversion,
		/// it'll look something like...
		/// obj => Convert(obj.Property) [e.g., int -> object]
		/// OR:
		/// obj => ConvertChecked(obj.Property) [e.g., int -> long]
		/// ...which are the cases checked in IsConversion
		/// </remarks>
		public static bool IsConversion( this System.Linq.Expressions.Expression exp )
		{
			return (
				exp.NodeType == ExpressionType.Convert ||
				exp.NodeType == ExpressionType.ConvertChecked
			);
		}
	}
}