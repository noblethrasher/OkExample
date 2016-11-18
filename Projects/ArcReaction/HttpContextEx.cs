using System;
using System.Web;

namespace ArcReaction
{
    public sealed class HttpContextEx
    {
        HttpContextBase context;

        public HttpContextEx(HttpContextBase context)
        {
            this.context = context;
        }

        public HttpContextEx(HttpContext context)
            : this(new HttpContextWrapper(context))
        {

        }

        public ArcReactionUser User
        {
            get
            {
                return context.User as ArcReactionUser;
            }
        }

        public HttpRequestBase Request
        {
            get
            {
                return context.Request;
            }
        }

        public HttpResponseBase Response
        {
            get
            {
                return context.Response;
            }
        }

        public string this[string name]
        {
            get
            {
                return context.Request[name];
            }
        }

        public HttpServerUtilityBase Server
        {
            get
            {
                return context.Server;
            }
        }
    }
}