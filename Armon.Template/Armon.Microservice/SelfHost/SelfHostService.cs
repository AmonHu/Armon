using log4net;
using Microsoft.Owin.Hosting;
using Armon.Microservice.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armon.Microservice.SelfHost
{
    internal class SelfHostService
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SelfHostService));
        private IDisposable _serverDisposable;

        /// <summary>
        /// 启动微服务
        /// </summary>
        public void Start()
        {
            _serverDisposable = WebApp.Start(string.Format("http://*:{0}", ConfigHelper.Instance.WebPort));
            _log.InfoFormat("Selfhost service started on {0}!\n", ConfigHelper.Instance.WebPort);

            ObjectStore.Instance.Initalize();
        }

        public void Stop()
        {
            if (_serverDisposable != null)
            {
                _serverDisposable.Dispose();
            }
        }
    }
}