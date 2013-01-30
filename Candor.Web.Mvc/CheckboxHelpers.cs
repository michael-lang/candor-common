using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace Candor.Web.Mvc
{
    /// <summary>
    /// Helper methods for working with checkboxes.
    /// </summary>
    public static class CheckboxHelpers
    {
        /// <summary>
        /// Creates a checkbox list with an item datasource of a list of enhanced select list items.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="html"></param>
        /// <param name="expression">A lambda expression of the property to bind to the checkbox list.</param>
        /// <param name="items">The datasource for the checkbox items.</param>
        /// <returns></returns>
        public static MvcHtmlString EnhancedCheckBoxListFor<TModel, TProperty>(
            this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, List<EnhancedSelectListItem> items)
        {
            if (items == null || items.Count == 0)
                return MvcHtmlString.Create("");

            string name = ((expression.Body as MemberExpression).Member as System.Reflection.PropertyInfo).Name;
            string fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

            string result = string.Empty;
            foreach (EnhancedSelectListItem item in items)
            {
                RouteValueDictionary attributes = new RouteValueDictionary(new { id = (fullName + item.Value).Replace(".", "_"), title = item.Title, @class = item.Class, value = item.Value, name = fullName });
                RouteValueDictionary dataAttributes = new RouteValueDictionary(item.DataAttributes);
                foreach (var attr in dataAttributes)
                    attributes.Add("data-" + attr.Key, attr.Value);

                var checkTag = CheckboxHelpers.GetCheckboxTag(item.Selected, attributes);

                result += checkTag.ToString(TagRenderMode.Normal); //html.CheckBox(name, item.Selected, attributes).ToHtmlString();

                TagBuilder labelTag = new TagBuilder("label");
                labelTag.MergeAttribute("for", name + item.Value);
                labelTag.InnerHtml = item.Text;
                result += labelTag.ToString(TagRenderMode.Normal);
            }
            return MvcHtmlString.Create(result);
        }
        /// <summary>
        /// Creates a disabled checkbox list
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString DisabledCheckBoxFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool>> expression, IDictionary<string, Object> htmlAttributes)
        {
            ModelMetadata modelMetadata = ModelMetadata.FromLambdaExpression<TModel, bool>(expression, helper.ViewData);

            string result = String.Empty;

            bool isChecked = false;
            bool parsedValue;
            if (modelMetadata.Model != null && bool.TryParse(modelMetadata.Model.ToString(), out parsedValue))
                isChecked = parsedValue;

            string name = ExpressionHelper.GetExpressionText((LambdaExpression)expression);

            TagBuilder checkbox = CheckboxHelpers.GetCheckboxTag(isChecked, htmlAttributes);
            checkbox.MergeAttribute("disabled", "true", true);
            checkbox.MergeAttribute("name", name);
            checkbox.MergeAttribute("id", name);
            result += checkbox.ToString(TagRenderMode.Normal);

            TagBuilder hidden = new TagBuilder("input");
            hidden.Attributes.Add("type", "hidden");
            hidden.Attributes.Add("value", isChecked.ToString());
            hidden.Attributes.Add("name", name);
            hidden.Attributes.Add("id", name);

            result += hidden.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(result);
        }
        /// <summary>
        /// Creates a checkbox tag.
        /// </summary>
        /// <param name="isChecked"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        internal static TagBuilder GetCheckboxTag(bool isChecked, IDictionary<string, Object> attributes)
        {
            TagBuilder checkTag = new TagBuilder("input");
            checkTag.Attributes.Add("type", "checkbox");
            checkTag.MergeAttributes(attributes);
            if (isChecked)
                checkTag.Attributes.Add("checked", "checked");
            return checkTag;
        }
    }
}
