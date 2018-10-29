using log4net;
using Armon.Microservice.Local;
using Armon.Microservice.SelfHost;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Armon.Microservice
{
    internal class ServicesHost
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ServicesHost));

        public ServicesHost()
        {
            ServiceList = new List<object>()
            {
                LocalService.Instance,
                new SelfHostService(),
                //NetMQ.NetMQService.Instance,
            };
        }

        public IEnumerable<Object> ServiceList { get; private set; }

        public void Start()
        {
            foreach (var service in ServiceList)
            {
                try
                {
                    service.GetType().InvokeMember("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, service, null);
                }
                catch (Exception ex)
                {
                    _log.Error(service.GetType().Name + " started error!", ex);
                }
            }
        }

        public void Stop()
        {
            foreach (var service in ServiceList)
            {
                try
                {
                    service.GetType().InvokeMember("Stop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, service, null);
                }
                catch (Exception ex)
                {
                    _log.Error(service.GetType().Name + " stoped error!", ex);
                }
            }
        }
    }
}