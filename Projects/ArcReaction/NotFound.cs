using System.Web;

namespace ArcReaction
{
    public sealed class NotFound : AppState
    {
        public override Representation GetRepresentation(HttpContextEx context)
        {
            return new AdHocHttpHandler(c => { c.Response.Write("not found."); c.Response.StatusCode = 404; });
        }
    }

    public sealed class Found : AppState
    {
        readonly Representation redirect;

        public Found(string location)
        {
            redirect = new AdHocRepresentation(c => { c.Response.AddHeader("Location", location); c.Response.StatusCode = 302; });
        }

        public override Representation GetRepresentation(HttpContextEx context)
        {
            return redirect;
        }
    }

    public sealed class Redirect : AppState
    {
        readonly Representation redirect;

        public Redirect(string location, int code)
        {
            redirect = new AdHocRepresentation(c => { c.Response.RedirectLocation = location; c.Response.StatusCode = code; });
        }

        public override Representation GetRepresentation(HttpContextEx context)
        {
            return redirect;
        }
    }
}