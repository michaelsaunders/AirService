using System;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(AirService.WebServices.Log4Net), "PreStart")]

namespace AirService.WebServices {
    public static class Log4Net
	{
        public static void PreStart()
		{
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}