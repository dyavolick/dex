using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace dex_webapp.Helpers
{
    public class QueryStringHelper
    {
        public static string ToQueryString(object parameters)
        {
            var fields = new NameValueCollection();

            parameters.GetType().GetProperties()
                .ToList()
                .ForEach(pi => fields.Add(pi.Name, pi.GetValue(parameters, null)?.ToString() ?? ""));

            var array = (from key in fields.AllKeys
                         from value in fields.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return "?" + string.Join("&", array);
        }
    }
}
