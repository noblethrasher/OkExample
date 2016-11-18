using System;
using System.Web;
using System.Web.UI;

namespace ArcReaction
{
    public abstract class ModelView<T> : Page
    {
        public T Model { get; set; }

        public static ModelView<T> Create(string path, T model)
        {
            var page = (ModelView<T>)PageParser.GetCompiledPageInstance(path, HttpContext.Current.Server.MapPath(path), HttpContext.Current);

            page.Model = model;

            return page;
        }

        public static ModelView<T> Create(string path, T model, HttpContextEx context)
        {
            var page = (ModelView<T>)PageParser.GetCompiledPageInstance(path, context.Server.MapPath(path), HttpContext.Current);

            page.Model = model;

            return page;
        }
        
        
        public static implicit operator ModelView<T>(Tuple<string, T> p)
        {
            var page = (ModelView<T>) PageParser.GetCompiledPageInstance(p.Item1, HttpContext.Current.Server.MapPath(p.Item1), HttpContext.Current);

            page.Model = p.Item2;

            return page;
        }
    }
}