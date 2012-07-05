using System;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof($rootnamespace$.Log4Net), "PreStart")]

namespace $rootnamespace$ {
    public static class Log4Net
	{
        public static void PreStart()
		{
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}