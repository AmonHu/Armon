using Armon.Microservice.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Armon.Microservice.Local
{
    /// <summary>
    /// 报文标志
    /// </summary>
    public enum EnumDatagramFlag : byte
    {
        Hello = 0x01,
        SetTime = 0x02,
        GetTime = 0x03,
        SetNetWorkPara = 0x04,
        GetNetWorkPara = 0x05,
        SetPara = 0x06,
        GetPara = 0x07,
        SetSmoothingTime = 0x08,
        GetSmoothingTime = 0x09,

        GetHistoryDoseRate = 0x60,
        GetHistoryTemperature = 0x61,
        GetHistorySysErrCode = 0x62,
        GetHistorySecondCnt = 0x63,
        GetHsitoryHv = 0x64,
        GetHistoryRh = 0x65,
        GetHistoryFeeErrCode = 0x66,

        GetDashboard = 0x20,
    }

    /// <summary>
    /// 报文
    /// </summary>
    internal class Datagram
    {
        /// <summary>
        /// 爱笑长度
        /// </summary>
        public const int MinLength = 5;

        public const byte Crc = 0xA5;

        /// <summary>
        /// 开始标志
        /// </summary>
        public const byte StartFlag = (byte)'s';

        /// <summary>
        /// 结束标志
        /// </summary>
        public const byte EndFlag = (byte)'t';

        /// <summary>
        /// 报文长度
        /// </summary>
        public int ContentLength
        {
            get { return Content == null ? Content.Length : 0; }
        }

        /// <summary>
        /// 报文标志
        /// </summary>
        public EnumDatagramFlag Flag { get; private set; }

        /// <summary>
        /// 报文内容
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="flag">报文标志</param>
        /// <param name="content">报文</param>
        public Datagram(EnumDatagramFlag flag, byte[] content = null)
        {
            this.Flag = flag;
            this.Content = content;
        }

        /// <summary>
        /// 格式化报文
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            byte[] buffer = new byte[3 + ContentLength];
            buffer[0] = (byte)buffer.Length;
            buffer[1] = (byte)Flag;
            if (ContentLength > 0)
            {
                Buffer.BlockCopy(Content, 0, buffer, 2, ContentLength);
            }
            buffer[buffer.Length - 1] = Crc;

            List<byte> result = new List<byte>() { StartFlag };
            result.AddRange(string.Join("", buffer.Select(e => e.ToString("X2"))).ToCharArray().Select(e => (byte)e));
            result.Add(EndFlag);

            return result.ToArray();
        }
    }

    /// <summary>
    /// 报文解析
    /// </summary>
    internal class DatagramResolver
    {
        /// <summary>
        /// 报文列表
        /// </summary>
        private readonly List<byte> _bytes = new List<byte>();

        /// <summary>
        /// 追加报文
        /// </summary>
        /// <param name="buffer">待追加报文</param>
        public void Append(byte[] buffer)
        {
            if (buffer == null) return;

            this.Append(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 追加报文
        /// </summary>
        /// <param name="buffer">待追加报文</param>
        /// <param name="offset">offset</param>
        /// <param name="length">长度</param>
        public void Append(byte[] buffer, int offset, int length)
        {
            if (buffer == null || offset < 0 || offset > buffer.Length - 1) return;
            _bytes.AddRange(buffer.Skip(offset).Take(Math.Min(length, buffer.Length - offset)));
        }

        /// <summary>
        /// 报文长度
        /// </summary>
        public int Count { get { return _bytes.Count; } }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            this._bytes.Clear();
        }

        /// <summary>
        /// 是否已添加
        /// </summary>
        public bool IsFill
        {
            get
            {
                return _bytes.Count >= 8 &&
                       _bytes.First() == Datagram.StartFlag && _bytes.Last() == Datagram.EndFlag
                       && _bytes.Count == 2 + 2 * (byte.Parse(string.Format("{0}{1}", (char)_bytes[1], (char)_bytes[2]), NumberStyles.HexNumber));
            }
        }

        /// <summary>
        /// 解析报文
        /// </summary>
        /// <returns></returns>
        public Datagram TryResolve()
        {
            if (!IsFill) return null;
            List<byte> buffer = new List<byte>();
            for (int i = 0; i < (_bytes.Count - 2) / 2; i++)
            {
                buffer.Add(byte.Parse(string.Format("{0}{1}", (char)_bytes[i * 2 + 1], (char)_bytes[i * 2 + 2]), NumberStyles.HexNumber));
            }

            Datagram result = new Datagram((EnumDatagramFlag)buffer[1], buffer[0] == 0x03 ? null : buffer.Skip(2).Take(buffer[0] - 3).ToArray());
            return result;
        }
    }
}