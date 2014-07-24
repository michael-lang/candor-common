using System;
using System.Text;
using System.Web.Mvc;

namespace Candor.Web.Mvc
{
    /// <summary>
    /// Extensions for ModelStateDictionary
    /// </summary>
    public static class ModelStateDictionaryExtension
    {
        /// <summary>
        /// Converts a model state dictionary into an html formatted string
        /// for a custom presentation.
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static String ToHtmlString(this ModelStateDictionary modelState)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in modelState.Keys)
            {
                foreach (var err in modelState[key].Errors)
                {
                    if (!string.IsNullOrWhiteSpace(err.ErrorMessage))
                        sb.AppendFormat("{0}<br/>", err.ErrorMessage);
                    else if (err.Exception != null)
                        sb.AppendFormat("{0}<br/>", err.Exception.Message);
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Converts a model state dictionary into a Json response
        /// for a custom presentation.
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static object ToJson(this ModelStateDictionary modelState)
        {
            dynamic anon = new { };
            foreach (var key in modelState.Keys)
            {
                anon[key] = new { name = key, value = "" };
                foreach (var err in modelState[key].Errors)
                {
                    if (!string.IsNullOrWhiteSpace(err.ErrorMessage))
                        anon[key].value += err.ErrorMessage;
                    else if (err.Exception != null)
                        anon[key].value += err.Exception.Message;
                }
            }
            return anon;
        }
    }
}