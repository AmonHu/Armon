using Armon.Microservice.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armon.Microservice
{
    /// <summary>
    /// 参数配置类
    /// </summary>
    public sealed class ConfigHelper
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public int WebPort { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 服务显示名称
        /// </summary>
        public string DisplayName { get; set; }

        private ConfigHelper()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            ServiceName = ConfigurationManager.AppSettings["ServiceName"];
            DisplayName = ConfigurationManager.AppSettings["DisplayName"];
            Description = ConfigurationManager.AppSettings["Description"];
            WebPort = Convert.ToInt32(ConfigurationManager.AppSettings["SelfHostPort"]);
        }

        /// <summary>
        /// 单例变量
        /// </summary>
        private static ConfigHelper _instance;
        /// <summary>
        /// 单例对象
        /// </summary>
        public static ConfigHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigHelper();
                return _instance;
            }
        }

        /// <summary>
        /// 更新config文件 key 对应的value 注意此方法只支持运行时更改，debug状态下看不到效果的
        /// </summary>
        /// <param name="values"></param>
        private static void UpdateSettings(Dictionary<string, string> values)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (var each in values)
            {
                configuration.AppSettings.Settings[each.Key].Value = each.Value;
            }
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="key">要更新key</param>
        /// <param name="value">写入的的值</param>
        private static void UpdateSettings(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}