using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
#pragma warning disable  CS1591

namespace Nistec.Web
{
    /// <summary>
    /// HttpExtension
    /// </summary>
    public static class HttpExtension
    {
        public static string GetReferrer(this HttpRequestBase Request)
        {
            if (Request == null)
                return "";
            string referer = Request.ServerVariables["HTTP_REFERER"];
            if (string.IsNullOrEmpty(referer))
                return Request.ServerVariables["HTTP_HOST"]; ;
            return referer;
        }
        public static string GetReferrer(this HttpRequest Request)
        {
            if (Request == null)
                return "";
            string referer = Request.ServerVariables["HTTP_REFERER"];
            if (string.IsNullOrEmpty(referer))
                return Request.ServerVariables["HTTP_HOST"]; ;
            return referer;
        }
    }


    /// <summary>
    /// UriExtensions
    /// </summary>
        /*
       public ActionResult BreadCrumb()
       {
        Uri referrer = Request.UrlReferrer;
        if (referrer != null && referrer.IsRouteMatch("Catalog", "Category"))
        {
         string categoryName = referrer.GetRouteParameterValue("categoryName");
        }
       }
       */
    public static class UriExtension
    {
        public static bool IsRouteMatch(this Uri uri, string controllerName, string actionName)
        {
            RouteInfo routeInfo = new RouteInfo(uri, HttpContext.Current.Request.ApplicationPath);
            return (routeInfo.RouteData.Values["controller"].ToString() == controllerName && routeInfo.RouteData.Values["action"].ToString() == actionName);
        }

        public static string GetRouteParameterValue(this Uri uri, string parameterName)
        {
            var routeInfo = new RouteInfo(uri, HttpContext.Current.Request.ApplicationPath);
            return routeInfo.RouteData.Values[parameterName] != null ? routeInfo.RouteData.Values[parameterName].ToString() : null;
        }
    }
    /// <summary>
    /// RouteInfo
    /// </summary>
    public class RouteInfo
    {
        public RouteInfo(Uri uri, string applicaionPath)
        {
            RouteData = RouteTable.Routes.GetRouteData(new InternalHttpContext(uri, applicaionPath));
        }

        public RouteData RouteData { get; private set; }

        private class InternalHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            public InternalHttpContext(Uri uri, string applicationPath)
            {
                _request = new InternalRequestContext(uri, applicationPath);
            }

            public override HttpRequestBase Request
            {
                get { return _request; }
            }
        }

        private class InternalRequestContext : HttpRequestBase
        {
            private readonly string _appRelativePath;
            private readonly string _pathInfo;

            public InternalRequestContext(Uri uri, string applicationPath)
            {
                _pathInfo = "";
                if (string.IsNullOrEmpty(applicationPath) || !uri.AbsolutePath.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase))
                {
                    _appRelativePath = uri.AbsolutePath.Substring(applicationPath.Length);
                }
                else
                {
                    _appRelativePath = uri.AbsolutePath;
                }
            }

            public override string AppRelativeCurrentExecutionFilePath
            {
                get { return string.Concat("~", _appRelativePath); }
            }

            public override string PathInfo
            {
                get { return _pathInfo; }
            }
        }
    }
}
