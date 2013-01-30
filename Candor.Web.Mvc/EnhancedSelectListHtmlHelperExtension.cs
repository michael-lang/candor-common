using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using Candor.Reflection;

namespace Candor.Web.Mvc
{
    /// <summary>
    /// HtmlHelper Extension methods for creating enhanced drop down lists.
    /// </summary>
    public static class EnhancedSelectListHtmlHelperExtension
    {
        /// <summary>
        /// Inserts a new blank item into the list.
        /// </summary>
        /// <param name="list"></param>
        public static void InsertBlank(this List<EnhancedSelectListItem> list)
        {
            list.Insert(0, new EnhancedSelectListItem() { Value = "", Text = "" });
        }
        /// <summary>
        /// Creates an enhanced drop down list with a specific html name attribute value.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="name">The value for the name attribute (also the id attribute if it is not in the htmlAttributes parameter.</param>
        /// <param name="selectEnumerable">The list of items to be added to the dropdown list.</param>
        /// <param name="htmlAttributes">Any other htmlAttributes to be applied to the dropdown (select element).</param>
        /// <remarks>
        /// If the name parameter has invalid characters like [] or . they will be automatically converted to _ for the id attribute value.
        /// This is to make it valid html.  The name value will be sent in html as specified; which is what is used for model binding.
        /// </remarks>
        /// <returns></returns>
        public static MvcHtmlString EnhancedDropDownList(this HtmlHelper html, string name, IEnumerable<EnhancedSelectListItem> selectEnumerable, object htmlAttributes)
        {
            var tagSelect = new TagBuilder("select");

            var htmlAttributesDictionary = html.AnonymousObjectToHtmlAttributesMvc2(htmlAttributes);
            tagSelect.MergeAttributes(htmlAttributesDictionary);
            if (tagSelect.Attributes.ContainsKey("name"))
                tagSelect.Attributes["name"] = name;
            else
                tagSelect.Attributes.Add("name", name);

            if (!tagSelect.Attributes.ContainsKey("id") && !string.IsNullOrEmpty(name))
                tagSelect.Attributes.Add("id", name.Replace(".", "_"));

            List<EnhancedSelectListItem> selectList;
            if (selectEnumerable == null)
                selectList = new List<EnhancedSelectListItem>();
            else if (selectEnumerable is List<EnhancedSelectListItem>)
                selectList = (List<EnhancedSelectListItem>)selectEnumerable;
            else
                selectList = selectEnumerable.ToList();
            var groupNames = new List<string>();
            selectList.ForEach(option => { if (!groupNames.Contains((option.Group ?? "").Trim())) { groupNames.Add((option.Group ?? "").Trim()); } });

            foreach (string groupName in groupNames)
            {	//group name is only unique if whitespace is not the only difference.
                List<EnhancedSelectListItem> groupOptions = selectList.FindAll(option => (option.Group ?? "").Trim() == groupName);
                TagBuilder tagGroup = null;
                if (!string.IsNullOrEmpty(groupName))
                {
                    tagGroup = new TagBuilder("optgroup");
                    tagGroup.Attributes.Add("label", groupOptions[0].Group); //add any whitespace back into group name
                }
                groupOptions.ForEach(groupOption =>
                {
                    var tagOption = new TagBuilder("option");
                    if (!groupOption.Enabled)
                        tagOption.Attributes.Add("disabled", "disabled");
                    if (!string.IsNullOrEmpty(groupOption.Class))
                        tagOption.Attributes.Add("class", groupOption.Class);
                    if (!string.IsNullOrEmpty(groupOption.Title))
                        tagOption.Attributes.Add("title", groupOption.Title);
                    if (groupOption.Selected)
                        tagOption.Attributes.Add("selected", "selected");
                    tagOption.Attributes.Add("value", groupOption.Value);
                    tagOption.InnerHtml = groupOption.Text;
                    RouteValueDictionary dictionary = html.AnonymousObjectToHtmlAttributesMvc2(groupOption.DataAttributes);
                    foreach (string key in dictionary.Keys)
                        tagOption.Attributes.Add("data-" + key, Convert.ToString(dictionary[key]));

                    if (tagGroup != null)
                        tagGroup.InnerHtml += tagOption.ToString(TagRenderMode.Normal);
                    else
                        tagSelect.InnerHtml += tagOption.ToString(TagRenderMode.Normal);
                });
                if (tagGroup != null)
                    tagSelect.InnerHtml += tagGroup.ToString(TagRenderMode.Normal);
            }

            return MvcHtmlString.Create(tagSelect.ToString(TagRenderMode.Normal));
        }
        /// <summary>
        /// Creates an enhanced dropdown list for a property specified by a lambda expression.  It sets the name and id attributes accordingly.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="html"></param>
        /// <param name="expression">A lamda expression of a property to set the name and id attribute values.</param>
        /// <param name="items">The list of items to be added to the dropdown list.</param>
        /// <param name="htmlAttributes">Any other htmlAttributes to be applied to the dropdown (select element).</param>
        /// <returns></returns>
        public static MvcHtmlString EnhancedDropDownListFor<TModel, TProperty>(
            this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<EnhancedSelectListItem> items, object htmlAttributes)
        {
            var tagSelect = new TagBuilder("select");

            string name = expression.GetFullPropertyName(".");
            string fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

            var htmlAttributesDictionary = html.AnonymousObjectToHtmlAttributesMvc2(htmlAttributes);
            if (htmlAttributesDictionary.ContainsKey("name"))
                htmlAttributesDictionary["name"] = fullName; //Name is generated with . as namespace separators
            else
                htmlAttributesDictionary.Add("name", fullName); //Name is generated with . as namespace separators
            if (!htmlAttributesDictionary.ContainsKey("id"))
                htmlAttributesDictionary.Add("id", fullName.Replace(".", "_")); //ID needs to be generated with _ as namespace separators
            tagSelect.MergeAttributes(htmlAttributesDictionary);

            List<EnhancedSelectListItem> selectList;
            if (items == null)
                selectList = new List<EnhancedSelectListItem>();
            else if (items is List<EnhancedSelectListItem>)
                selectList = (List<EnhancedSelectListItem>)items;
            else
                selectList = items.ToList();

            var selectedItem = selectList.Find(item => item.Selected);
            if (selectedItem == null)
            {
                var modelValue = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Model;
                if (modelValue != null)
                {
                    string modelString = modelValue.ToString();
                    selectedItem = selectList.Find(item => item.Value == modelString);
                    if (selectedItem != null)
                        selectedItem.Selected = true;
                }
            }

            var groupNames = new List<string>();
            selectList.ForEach(option => { if (!groupNames.Contains((option.Group ?? "").Trim())) { groupNames.Add((option.Group ?? "").Trim()); } });

            foreach (string groupName in groupNames)
            {	//group name is only unique if whitespace is not the only difference.
                List<EnhancedSelectListItem> groupOptions = selectList.FindAll(option => (option.Group ?? "").Trim() == groupName);
                TagBuilder tagGroup = null;
                if (!string.IsNullOrEmpty(groupName))
                {
                    tagGroup = new TagBuilder("optgroup");
                    tagGroup.Attributes.Add("label", groupOptions[0].Group); //add any whitespace back into group name
                }
                groupOptions.ForEach(groupOption =>
                {
                    var tagOption = new TagBuilder("option");
                    if (!groupOption.Enabled)
                        tagOption.Attributes.Add("disabled", "disabled");
                    if (!string.IsNullOrEmpty(groupOption.Class))
                        tagOption.Attributes.Add("class", groupOption.Class);
                    if (!string.IsNullOrEmpty(groupOption.Title))
                        tagOption.Attributes.Add("title", groupOption.Title);
                    if (groupOption.Selected)
                        tagOption.Attributes.Add("selected", "selected");
                    tagOption.Attributes.Add("value", groupOption.Value);
                    tagOption.InnerHtml = groupOption.Text;
                    RouteValueDictionary dictionary = html.AnonymousObjectToHtmlAttributesMvc2(groupOption.DataAttributes);
                    foreach (string key in dictionary.Keys)
                        tagOption.Attributes.Add("data-" + key, Convert.ToString(dictionary[key]));

                    if (tagGroup != null)
                        tagGroup.InnerHtml += tagOption.ToString(TagRenderMode.Normal);
                    else
                        tagSelect.InnerHtml += tagOption.ToString(TagRenderMode.Normal);
                });
                if (tagGroup != null)
                    tagSelect.InnerHtml += tagGroup.ToString(TagRenderMode.Normal);
            }
            return MvcHtmlString.Create(tagSelect.ToString(TagRenderMode.Normal));
        }
        /// <summary>
        /// Creates a checkbox list for the specified property and list of possible values.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="html"></param>
        /// <param name="expression">A lamda expression of a property to set the name and id attribute values.</param>
        /// <param name="items">The list of items for which each becomes a checkbox.</param>
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
                var attributes = new RouteValueDictionary(new { id = (fullName + item.Value).Replace(".", "_"), title = item.Title, @class = item.Class, value = item.Value, name = fullName });
                var dataAttributes = html.AnonymousObjectToHtmlAttributesMvc2(item.DataAttributes);
                foreach (var attr in dataAttributes)
                    attributes.Add("data-" + attr.Key, attr.Value);

                var checkTag = CheckboxHelpers.GetCheckboxTag(item.Selected, attributes);

                result += checkTag.ToString(TagRenderMode.Normal); //html.CheckBox(name, item.Selected, attributes).ToHtmlString();

                var labelTag = new TagBuilder("label");
                labelTag.MergeAttribute("for", name + item.Value);
                labelTag.InnerHtml = item.Text;
                result += labelTag.ToString(TagRenderMode.Normal);
            }
            return MvcHtmlString.Create(result);
        }
    }
}
