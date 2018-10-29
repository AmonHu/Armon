using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.ServiceModel.Activation;
using System.Web.Routing;
using Armon.RESTService.BookServices;

namespace Armon.RESTService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Register all the service routes here
            RouteTable.Routes.Add(new ServiceRoute("Book", new WebServiceHostFactory(), typeof(BookService)));
        }
    }
}