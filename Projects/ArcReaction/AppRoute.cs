using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public abstract class AppRoute : IHttpModule
    {
        HttpApplication app;

        public void Dispose() { }        

        public void Init(HttpApplication app) => (this.app = app).PostResolveRequestCache += MapHandler;

        protected abstract AppState GetRoot(HttpContextEx context);

        protected virtual AppState TranslateNull(AppState app_state, HttpContext context) => app_state ?? new NotFound();        

        protected virtual void MapHandler(object sender, EventArgs e)
        {
            var result = new RouteAnalysis(app.Context);
            var curr = (AppState) null;
            var ctx = new HttpContextEx(app.Context);
            var msg = new Message(ctx);
                    
            if (result.IsRoutable)
            {
                try
                {
                    curr = GetRoot(ctx);
                    
                    foreach (var path in result)
                    {
                        msg.Text = path;
                        curr = TranslateNull(curr.Next(msg), app.Context);
                    }
                }
                catch (ArcException ex)
                {
                    if (ex.CanHandle)
                        curr = ex.AppState;
                    else
                        throw;
                }

                app.Context.RemapHandler(curr.GetRepresentation(ctx));                
            }
        }

        struct RouteAnalysis : IEnumerable<string>
        {
            static readonly string[] splits = new[] { "/" };

            readonly List<string> segments, mime_types;

            public RouteAnalysis(HttpContext context) : this()
            {
                segments = new List<string>();
                
                var paths = context.Request.Path.Split(splits, StringSplitOptions.RemoveEmptyEntries);

                foreach (var path in paths)
                {
                    if (path[0] == '.')
                        (mime_types = mime_types ?? new List<string>()).Add(path);
                    else
                        segments.Add(path);
                }
            }

            public bool IsRoutable
            {
                get
                {
                    var last = segments.LastOrDefault();
                    
                    if(last != null)
                    {
                        var index = last.LastIndexOf('.');

                        return index < 0 || (index < last.Length - 1 && MimeMapping.GetMimeMapping(last.Substring(index)) == "application/octet-stream");
                    }

                    return true;
                }
            }

            public IEnumerator<string> GetEnumerator() => segments.GetEnumerator();            

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

    }
}
