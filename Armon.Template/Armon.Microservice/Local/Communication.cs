using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;

namespace Armon.Microservice.Local
{
    /// <summary>
    /// 通信接口
    /// </summary>
    internal interface ICommunication
    {
        /// <summary>
        /// 是否连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 最近活跃时间
        /// </summary>
        DateTime LastAliveTime { get; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns>是否成功</returns>
        bool Connect();

        /// <summary>
        /// 断开
        /// </summary>
        /// <returns>是否成功</returns>
        bool Disconnect();

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="request">报文</param>
        /// <returns></returns>
        Datagram Send(Datagram request);
    }

    /// <summary>
    /// Socket通信
    /// </summary>
    internal class SocketCommunication : ICommunication
    {
        /// <summary>
        /// Socket
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// 报文分解器
        /// </summary>
        private readonly DatagramResolver _resolver = new DatagramResolver();

        /// <summary>
        /// 最好活跃时间
        /// </summary>
        public DateTime LastAliveTime { get; private set; }

        /// <summary>
        /// Socket连接状态
        /// </summary>
        public bool IsConnected
        {
            get { return _socket != null && _socket.Connected; }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            IPAddress ip = IPAddress.Parse("169.254.6.100");
            IPEndPoint ipEnd = new IPEndPoint(ip, 27011);
            //定义套接字类型
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Connect(ipEnd);
            }
            catch (SocketException e)
            {
                Console.Write("Fail to connect server");
                Console.Write(e.ToString());
                return false;
            }

            return this.IsConnected;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            if (this.IsConnected)
            {
                _socket.Disconnect(false);
            }
            return !IsConnected;
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="request">发送报文</param>
        /// <returns>返回报文</returns>
        public Datagram Send(Datagram request)
        {
            if (request == null) return null;

            byte[] buffer = request.Serialize();
            int count = this._socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            Console.WriteLine("Send {0} bytes", count);
            buffer = new byte[256];
            Stopwatch watch = new Stopwatch();
            watch.Restart();
            while (watch.Elapsed.TotalSeconds <= 1)
            {
                int length = _socket.Receive(buffer);
                if (length > 0)
                {
                    _resolver.Append(buffer, 0, length);
                    if (_resolver.IsFill)
                    {
                        LastAliveTime = DateTime.Now;

                        var result = _resolver.TryResolve();
                        _resolver.Clear();
                        return result;
                    }
                }
            }
            _resolver.Clear();
            return null;
        }
    }

    /// <summary>
    /// 串口通信
    /// </summary>
    internal class SerialPortCommunication : ICommunication
    {
        /// <summary>
        /// 串口类
        /// </summary>
        private SerialPort _port;

        /// <summary>
        /// 报文分解器
        /// </summary>
        private readonly DatagramResolver _resolver = new DatagramResolver();

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected
        {
            get { return _port != null && _port.IsOpen; }
        }

        /// <summary>
        /// 最新活跃时间
        /// </summary>
        public DateTime LastAliveTime { get; private set; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                _port = new SerialPort("COM1", 19200);
                _port.Open();
            }
            catch (Exception e)
            {
                Console.Write("Fail to connect server");
                Console.Write(e.ToString());
                return false;
            }
            return IsConnected;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            if (this.IsConnected)
            {
                _port.Close();
            }
            return !this.IsConnected;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Datagram Send(Datagram request)
        {
            if (request == null)
                return null;

            byte[] buffer = request.Serialize();
            this._port.Write(buffer, 0, buffer.Length);
            Console.WriteLine("Send {0} bytes", _port.BytesToWrite);

            buffer = new byte[256];
            Stopwatch watch = new Stopwatch();
            watch.Restart();
            while (watch.Elapsed.TotalSeconds <= 1)
            {
                int length = _port.BytesToRead;
                _port.Read(buffer, 0, length);
                if (length > 0)
                {
                    _resolver.Append(buffer, 0, length);
                    if (_resolver.IsFill)
                    {
                        LastAliveTime = DateTime.Now;

                        var result = _resolver.TryResolve();
                        _resolver.Clear();
                        return result;
                    }
                }
            }
            _resolver.Clear();
            return null;
        }
    }
}