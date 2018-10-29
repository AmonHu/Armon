using log4net;
using Armon.Microservice;
using Armon.Microservice.Local;
using Armon.Microservice.Model;
using System;
using System.Collections.Generic;
using Topshelf;

namespace Armon.Microservice
{
    internal class Program
    {
        private static ILog _logger = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (string.Equals(Environment.CurrentDirectory, Environment.SystemDirectory,
                    StringComparison.OrdinalIgnoreCase))
            {
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            _logger.InfoFormat("{0} start".PadRight(40, '*').PadRight(41, '\n'), ConfigHelper.Instance.ServiceName);

            HostFactory.Run(x =>
            {
                x.Service<ServicesHost>(s =>
                {
                    s.ConstructUsing(name => new ServicesHost());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.StartAutomaticallyDelayed();
                x.SetDescription(ConfigHelper.Instance.Description);
                x.SetDisplayName(ConfigHelper.Instance.DisplayName);
                x.SetServiceName(ConfigHelper.Instance.ServiceName);
            });

            _logger.InfoFormat("{0} end".PadRight(40, '*').PadRight(41, '\n'), ConfigHelper.Instance.ServiceName);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                if (ex != null)
                {
                    _logger.ErrorFormat("[ErrorType]：{0}", ex.GetType().Name);
                    _logger.ErrorFormat("[ErrorMsg]：{0}", ex.Message);
                    _logger.ErrorFormat("[StackTrace]：{0}", ex.StackTrace);
                    if (ex.InnerException != null)
                        _logger.ErrorFormat("[InnerException]:{0}", ex.InnerException);
                }
            }
            catch (Exception err)
            {
                _logger.ErrorFormat("UnhandledException Error Msg = {0}", err);
            }
        }
    }
}