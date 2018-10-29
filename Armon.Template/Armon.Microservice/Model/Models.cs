using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Armon.Microservice.Local;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Armon.Microservice.Model
{
    /// <summary>
    /// 实体接口
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty(propertyName: "recordId")]
        Guid RecordId { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [JsonProperty(propertyName: "createAt")]
        DateTime CreateAt { get; set; }

        /// <summary>
        /// 获取时间
        /// </summary>
        /// <returns>DateTime</returns>
        DateTime GetTime();
    }

    /// <summary>
    /// 实体基类
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// ID
        /// </summary>
        public virtual Guid RecordId { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public virtual DateTime CreateAt { get; set; }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetTime()
        {
            return this.CreateAt;
        }
    }

    /// <summary>
    /// 设备相关属性
    /// </summary>
    public class Nameplate
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        [JsonProperty("deviceType")]
        public string DeviceType { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        [JsonProperty("firmwareVersion")]
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        [JsonProperty("hardwareVersion")]
        public string HardwareVersion { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }//软件版本
    }

    public class ServiceConfig : BaseEntity
    {
        [JsonProperty("baudRate")]
        public virtual int BaudRate { get; set; }

        [JsonProperty("portName")]
        public virtual string PortName { get; set; }

        public virtual bool IsValid(out string error)
        {
            error = String.Empty;
            return true;
        }
    }

    /// <summary>
    /// 工作状态
    /// </summary>
    public enum EnumWorkStatus : byte
    {
        /// <summary>
        /// 未知
        /// </summary>
        None = 0x00,

        /// <summary>
        /// 连接失败
        /// </summary>
        Disconnected = 0x01,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0x02,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 0x03,
    }

    public class Dashboard : BaseEntity
    {
        [JsonProperty("value")]
        public virtual int Value { get; set; }
    }
}