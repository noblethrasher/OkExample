using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace ArcReaction
{
    public abstract class AsynRepresentation : Representation, IHttpAsyncHandler
    {
        public abstract IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData);
        public abstract void EndProcessRequest(IAsyncResult result);
    }

    


    public abstract class Representation : IHttpHandler
    {
        readonly bool validate_request;

        protected Representation()
        {
            
        }

        protected Representation(bool validate_request)
        {
            this.validate_request = validate_request;
        }
        
        public virtual bool IsReusable
        {
            get { return false; }
        }

        public abstract void ProcessRequest(HttpContext context);

        public static implicit operator Representation(string s)
        {
            if (s.StartsWith("/") && s.EndsWith(".aspx"))
                return new ASPXRepresentation(s);
            
            
            return new SimpleStringRepresentation(s);
        }
       
        public static implicit operator Representation(JSON[] xs)
        {
            return (JSON)xs;
        }

        public static implicit operator Representation(AdHocHttpHandler handler)
        {
            return new AdHocRepresentation(handler);
        }

        public static Representation Create(IHttpHandler handler)
        {
            return new SimpleHttpRepresentation(handler);
        }
    }

    public sealed class AdHocRepresentation : Representation
    {
        readonly AdHocHttpHandler handler;

        public AdHocRepresentation(Action<HttpContext> process_request)
        {
            this.handler = new AdHocHttpHandler(process_request);
        }

        public AdHocRepresentation(AdHocHttpHandler handler)
        {
            this.handler = handler;
        }

        public override void ProcessRequest(HttpContext context)
        {
            handler.ProcessRequest(context);
        }
    }

    internal sealed class SimpleHttpRepresentation : Representation
    {
        readonly IHttpHandler handler;

        public SimpleHttpRepresentation(IHttpHandler handler)
        {
            this.handler = handler;
        }

        public override void ProcessRequest(HttpContext context)
        {
            handler.ProcessRequest(context);
        }
    }

    internal sealed class ASPXRepresentation : Representation
    {
        readonly string path;

        public ASPXRepresentation(string path)
        {
            this.path = path;
        }

        public override void ProcessRequest(HttpContext context)
        {
            PageParser.GetCompiledPageInstance(path, context.Server.MapPath(path), context).ProcessRequest(context);
        }
    }

    internal class SimpleStringRepresentation : Representation
    {
        readonly string str;

        public SimpleStringRepresentation(string str)
        {
            this.str = str;
        }

        public override void ProcessRequest(HttpContext context)
        {
            context.Response.Write(str);
        }
    }
}