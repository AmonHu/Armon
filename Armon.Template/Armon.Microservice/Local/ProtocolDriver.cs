using Armon.Microservice.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Armon.Microservice.Local
{
    /// <summary>
    /// 驱动接口
    /// </summary>
    internal interface IDriver
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// 设备状态
        /// </summary>
        EnumWorkStatus Status { get; }

        /// <summary>
        /// 是否连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        DateTime LastAliveTime { get; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// 断开
        /// </summary>
        /// <returns></returns>
        bool Disconnect();

        /// <summary>
        /// 握手
        /// </summary>
        /// <returns></returns>
        bool Hello();

        /// <summary>
        /// 获取设备时间
        /// </summary>
        /// <param name="value">返回的设备时间</param>
        /// <returns>是否成功</returns>
        bool RetrieveTime(out DateTime value);

        /// <summary>
        /// 设置设备时间
        /// </summary>
        /// <param name="time">目标时间</param>
        /// <returns>是否成功</returns>
        bool SetDateTime(DateTime time);

        Dashboard RetrieveDashboard();
    }

    /// <summary>
    /// 虚拟驱动类
    /// </summary>
    internal class DemoDriver : IDriver
    {
        public DemoDriver()
        {
            //this.Comm = new SocketCommunication();
        }

        /// <summary>
        /// 通信方式
        /// </summary>
        protected ICommunication Comm { get; set; }

        /// <summary>
        /// 固件版本
        /// </summary>
        public string FirmwareVersion { get; private set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HardwareVersion { get; private set; }

        /// <summary>
        /// 设备名
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 工作状态
        /// </summary>
        public EnumWorkStatus Status
        {
            get
            {
                if (!this.IsConnected)
                    return EnumWorkStatus.Disconnected;
                return this.HasError ? EnumWorkStatus.Error : EnumWorkStatus.Normal;
            }
        }

        /// <summary>
        /// 是否错误
        /// </summary>
        private bool HasError { get; set; }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime LastAliveTime
        {
            get { return Comm == null ? Comm.LastAliveTime : DateTime.MinValue; }
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected
        {
            get { return Comm != null && Comm.IsConnected; }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            return Comm != null && Comm.Connect();
        }

        /// <summary>
        /// 断开
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            return Comm == null || Comm.Disconnect();
        }

        /// <summary>
        /// 握手
        /// </summary>
        /// <returns></returns>
        public bool Hello()
        {
            if (!this.IsConnected)
                return false;

            var reply = this.Comm.Send(new Datagram(EnumDatagramFlag.Hello));
            if (reply == null || reply.Flag != EnumDatagramFlag.Hello + 0x80 || reply.ContentLength != 12)
                return false;

            this.FirmwareVersion = string.Format("V{0}", reply.Content[1]);
            this.HardwareVersion = string.Format("V{0}R{1}", reply.Content[3], reply.Content[2]);

            return true;
        }

        /// <summary>
        /// 获取设备时间
        /// </summary>
        /// <param name="value">返回的设备时间</param>
        /// <returns>是否成功</returns>
        public bool RetrieveTime(out DateTime value)
        {
            value = DateTime.MinValue;
            if (!this.IsConnected) return false;

            var reply = this.Comm.Send(new Datagram(EnumDatagramFlag.GetTime));
            if (reply == null || reply.Flag != EnumDatagramFlag.GetTime + 0x80 || reply.ContentLength != 7)
                return false;

            value = reply.Content.ToDateTime7_Ex();

            return value != DateTime.MinValue;
        }

        /// <summary>
        /// 设置设备时间
        /// </summary>
        /// <param name="time">目标时间</param>
        /// <returns>是否成功</returns>
        public bool SetDateTime(DateTime time)
        {
            if (!this.IsConnected) return false;
            var reply = this.Comm.Send(new Datagram(EnumDatagramFlag.SetTime, time.ToBuffer7_Ex()));
            if (reply == null || reply.Flag != EnumDatagramFlag.SetTime + 0x80 || reply.ContentLength != 0)
                return false;

            return true;
        }

        private Random _random = new Random(DateTime.Now.Millisecond);

        public Dashboard RetrieveDashboard()
        {
            return new Dashboard() { Value = _random.Next(1, ushort.MaxValue) };
        }
    }
}