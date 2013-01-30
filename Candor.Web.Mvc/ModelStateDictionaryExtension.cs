using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Candor.Web.Mvc
{
    public static class ModelStateDictionaryExtension
    {
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