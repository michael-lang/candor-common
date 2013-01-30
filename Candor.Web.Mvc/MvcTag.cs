using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Candor.Web.Mvc
{

    /// <summary>
    /// Defines any type of tag that can be used in a using statement which will automatically close itself at the end of the using block (on Dispose).
    /// </summary>
    public class MvcTag : IDisposable
    {
        private bool _disposed;
        private readonly ViewContext _viewContext;
        private readonly TextWriter _writer;
        private readonly string _tagName;

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        [Obsolete("The recommended alternative is the constructor MvcTag(ViewContext viewContext).", true /* error */)]
        public MvcTag(HttpResponseBase httpResponse)
        {
            if (httpResponse == null)
            {
                throw new ArgumentNullException("httpResponse");
            }

            _writer = httpResponse.Output;
        }

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        public MvcTag(ViewContext viewContext, string tagName)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException("tagName");
            }

            _viewContext = viewContext;
            _writer = viewContext.Writer;
            _tagName = tagName.Trim();
            if (_tagName.ToLower() == "form")
                throw new ArgumentOutOfRangeException("tagName", "Use MvcForm for a form tag.");
        }
        /// <summary>
        /// Disposes the tag instance by closing it.
        /// </summary>
        public void Dispose()
        {
            Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the tag instance by closing it.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                _writer.Write("</" + _tagName + ">");
            }
        }
        /// <summary>
        /// Ends the tag by disposing this instance.
        /// </summary>
        public void EndTag()
        {
            Dispose(true);
        }
    }
}
