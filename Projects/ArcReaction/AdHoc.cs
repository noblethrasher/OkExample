using System;
using System.Web;

namespace ArcReaction
{
    public sealed class AdHocHttpHandler : IHttpHandler
    {
        readonly Action<HttpContext> process_request;
        readonly Func<bool> is_resusable;

        readonly static Func<bool> default_answer = delegate { return false; };

        public AdHocHttpHandler(Action<HttpContext> process_request, Func<bool> is_reusable = null)
        {
            this.process_request = process_request;
            this.is_resusable = is_reusable;
        }

        public bool IsReusable
        {
            get { return (is_resusable ?? default_answer)(); }
        }

        public void ProcessRequest(HttpContext context)
        {
            process_request(context);
        }
    }
}