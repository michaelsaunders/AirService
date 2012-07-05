using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using AirService.WebServices.Framework;
using Ninject;
using Ninject.Extensions.Wcf;
using log4net;

namespace AirService.WebServices
{
    public class AirServiceWebServiceApplication : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            this.RegisterRoutes();
            KernelContainer.Kernel = new StandardKernel(new WebServiceNinjectModule());
            ILog logger = LogManager.GetLogger(typeof (AirServiceWebServiceApplication));
            logger.Info("Application started.");
        } 

        private void RegisterRoutes()
        {
            // Using NinjectServiceHostFactory fails when posting json object 
            // Receives HTTP/1.1 415 Cannot process the message because the content type 'application/json; charset=utf-8' was not the expected type 'text/xml; charset=utf-8'. 
            // 
            // Unable to use NinjectServiceHostFactory force services to be manually resolved. 
            // var factory = new NinjectServiceHostFactory();
            // Cannot use Constructor DI on Restful WebService... 
            var factory = new WebServiceHostFactory();
            RouteTable.Routes.Add(new ServiceRoute("Customer",
                                                   factory,
                                                   typeof (CustomerWebService)));
            RouteTable.Routes.Add(new ServiceRoute("Venue",
                                                   factory,
                                                   typeof (VenueWebService)));
        }

 
    }
}