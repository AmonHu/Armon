using Armon.Microservice.Model;
using Armon.Microservice.NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Armon.Microservice.Local
{
    /// <summary>
    /// 本地服务负责和设备交互
    /// </summary>
    internal class LocalService
    {
        private static readonly LocalService _instance = new LocalService();

        /// <summary>
        /// 日志
        /// <summary>
        private log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(LocalService));

        /// <summary>
        /// 握手
        /// </summary>
        private readonly System.Timers.Timer helloTimer = new System.Timers.Timer(10 * 1000);

        /// <summary>
        /// 接收数据
        /// </summary>
        private System.Timers.Timer retrieveTimer = new System.Timers.Timer(2 * 1000);

        /// <summary>
        /// 驱动
        /// </summary>
        private IDriver Driver { get; set; }

        public ServiceConfig Config { get; private set; }

        public EnumWorkStatus Status { get; private set; }

        /// <summary>
        /// 单例对象
        /// </summary>
        public static LocalService Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        private LocalService()
        {
            //初始化
            Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            this.Config = ObjectStore.Instance.GetObjects<ServiceConfig>(limit: 1).FirstOrDefault();
            if (this.Config == null)
            {
                this.Config = CreateDefaultConfig();
                if (!ObjectStore.Instance.InsertObject(this.Config))
                {
                    _logger.ErrorFormat("Insert config to database failed.");
                }
            }

            this.Driver = new DemoDriver();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            try
            {
                this.Initialize();

                HockEvents(true);

                Driver.Connect();

                helloTimer.Enabled = true;
                retrieveTimer.Enabled = true;

                _logger.InfoFormat("Local service started.");
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("could not start the Local service error:{0}", e);
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            helloTimer.Enabled = false;
            retrieveTimer.Enabled = false;

            HockEvents(false);

            Driver.Disconnect();

            _logger.InfoFormat("Local service stopped.");
        }

        /// <summary>
        /// 事件注册和注销
        /// </summary>
        /// <param name="flag">标志</param>
        private void HockEvents(bool flag)
        {
            if (flag)
            {
                //workerTimer.Elapsed += workerTimer_Elapsed;
                helloTimer.Elapsed += helloTimer_Elapsed;
                retrieveTimer.Elapsed += retrieveTimer_Elapsed;
            }
            else
            {
                //workerTimer.Elapsed -= workerTimer_Elapsed;
                helloTimer.Elapsed -= helloTimer_Elapsed;
                retrieveTimer.Elapsed -= retrieveTimer_Elapsed;
            }
        }

        /// <summary>
        /// 驱动是否正常
        /// </summary>
        /// <returns>true/false</returns>
        private bool IsDriverAlive()
        {
            return Driver.IsConnected &&
                   (DateTime.Now - Driver.LastAliveTime).TotalSeconds <= 3 * 10;
        }

        /// <summary>
        /// helloTimer_Elapsed事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helloTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsDriverAlive())
            {
                _logger.WarnFormat("Sampler is not alive, stop and restart it.");
                Stop();
                Start();
            }
        }

        /// <summary>
        /// retrieveTimer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void retrieveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dashboard dashbaord = Driver.RetrieveDashboard();
            ObjectStore.Instance.DashboardCache.InsertObject(dashbaord);

            NetMQService.Instance.Publish("Dashboard", dashbaord);
        }

        /// <summary>
        /// 设备相关信息
        /// </summary>
        public Nameplate Nameplate
        {
            get
            {
                return new Nameplate()
                {
                    DeviceName = Driver.DeviceName,
                    DeviceType = "Demo",
                    FirmwareVersion = ((DemoDriver)Driver).FirmwareVersion,
                    HardwareVersion = ((DemoDriver)Driver).HardwareVersion,
                    Version = this.GetType().Assembly.GetName().Version.ToString()
                };
            }
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns></returns>
        private ServiceConfig CreateDefaultConfig()
        {
            ServiceConfig config = new ServiceConfig()
            {
                BaudRate = 19200,
                PortName = "COM1",
            };

            ObjectStore.Instance.InsertObject(config);
            return config;
        }

        public Dashboard GetDashboard()
        {
            return new Dashboard();
        }

        public DateTime GetDeviceTime()
        {
            return DateTime.Now;
        }

        public bool SetDeviceTime(DateTime time)
        {
            return true;
        }

        public bool SetServiceConfig(ServiceConfig config)
        {
            config.RecordId = Guid.NewGuid();
            config.CreateAt = DateTime.Now;

            if (ObjectStore.Instance.InsertObject(config))
            {
                this.Config = config;

                _logger.InfoFormat("Try to stop local service after reset config");

                this.Stop();
                this.Start();

                return true;
            }
            return false;
        }
    }
}