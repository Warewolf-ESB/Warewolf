﻿using System.Web.Mvc;

namespace Dev2.Web2
{
    public class FilterConfig
    {
        protected FilterConfig()
        {
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
