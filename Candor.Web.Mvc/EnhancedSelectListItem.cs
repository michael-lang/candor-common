using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace Candor.Web.Mvc
{
    /// <summary>
    /// An enhancement of a basic SelectListItem that adds other supported html attributes
    /// and extensions for the data specification.
    /// </summary>
    public class EnhancedSelectListItem : SelectListItem
    {
        private bool _enabled = true;
        /// <summary>
        /// Gets or sets if this item is selectable.
        /// </summary>
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        /// <summary>
        /// Gets or sets class names to be applied to the element representing this item.
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// Gets or sets a title (tooltip) to show for this item.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets a logical grouping for this item.  See group attribute on html Select tag in the W3C Html spec.
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Gets or sets a series of data attributes to be applied to the element for this item.  The actual html attribute
        /// name will get an additional "data-" in front of the name when output to html to meet the "data-" specification (see jQuery docs).
        /// </summary>
        public object DataAttributes { get; set; }
        /// <summary>
        /// Gets a set of select list items for a boolean type.
        /// </summary>
        /// <param name="selectedValue">The boolean value to be selected.</param>
        /// <returns>A new list of items.</returns>
        public static List<EnhancedSelectListItem> ListFromBoolean(bool selectedValue)
        {
            var items = new List<EnhancedSelectListItem>();
            items.Add(new EnhancedSelectListItem() { Value = "false", Text = "No", Selected = !selectedValue });
            items.Add(new EnhancedSelectListItem() { Value = "true", Text = "Yes", Selected = selectedValue });
            return items;
        }
        /// <summary>
        /// Creates a list of enhanced select list items for all values in a specific enum type.
        /// </summary>
        /// <param name="enumType">The type of the enumeration.</param>
        /// <returns></returns>
        public static List<EnhancedSelectListItem> ListFromEnum(Type enumType)
        {
            return ListFromEnum(enumType, new List<string>());
        }
        /// <summary>
        /// Creates a list of enhanced select list items for all values in a specific enum type.
        /// </summary>
        /// <param name="enumType">The type of the enumeration.</param>
        /// <param name="selectedValues">A comma separated string of values to be considered selected in the returned list.</param>
        /// <returns></returns>
        public static List<EnhancedSelectListItem> ListFromEnum(Type enumType, string selectedValues)
        {
            List<string> values = null;
            if (!string.IsNullOrWhiteSpace(selectedValues))
                values = new List<string>(selectedValues.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            else
                values = new List<string>();
            return ListFromEnum(enumType, values);
        }
        /// <summary>
        /// Creates a list of enhanced select list items for all values in a specific enum type.
        /// </summary>
        /// <param name="enumType">The type of the enumeration.</param>
        /// <param name="selectedValues">A list of string values to be considered selected in the returned list.</param>
        /// <returns></returns>
        public static List<EnhancedSelectListItem> ListFromEnum(Type enumType, List<string> selectedValues)
        {
            var items = new List<EnhancedSelectListItem>();
            if (selectedValues == null)
                selectedValues = new List<string>();

            foreach (var e in Enum.GetValues(enumType))
            {
                string eDisplay = e.ToString().CamelCaseToPhrase();
                System.Reflection.MemberInfo[] memInfo = enumType.GetMember(e.ToString());
                if (memInfo != null && memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                        eDisplay = ((System.ComponentModel.DataAnnotations.DisplayAttribute)attrs[0]).Name;
                }

                items.Add(new EnhancedSelectListItem()
                {
                    Text = eDisplay,
                    Value = ((int)e).ToString(CultureInfo.InvariantCulture),
                    Selected = selectedValues.Exists(s => s == ((int)e).ToString(CultureInfo.InvariantCulture) || s == eDisplay)
                });
            }
            return items;
        }
    }
}
