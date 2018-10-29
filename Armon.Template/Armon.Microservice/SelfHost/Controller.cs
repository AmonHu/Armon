using Newtonsoft.Json.Linq;
using Armon.Microservice.Local;
using Armon.Microservice.Model;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Armon.Microservice.SelfHost
{
    [RoutePrefix("api/device/demo")]
    public class DemoController : ApiController
    {
        private LocalService Service
        {
            get { return LocalService.Instance; }
        }

        /// <summary>
        /// 获取设备及服务铭牌
        /// </summary>
        /// <returns>Nameplate</returns>
        [HttpGet, Route("")]
        public IHttpActionResult GetNameplate()
        {
            return this.Ok(Service.Nameplate);
        }

        /// <summary>
        /// 设置设备名称
        /// 有问题
        /// </summary>
        /// <param name="jObject">Json对象</param>
        /// <returns>Nameplate</returns>
        [HttpPut, Route("")]
        public IHttpActionResult PutDeviceName([FromBody]JObject jObject)
        {
            if (jObject?["name"] == null || 
                string.IsNullOrEmpty((string)jObject["name"]))
                return this.BadRequest("Cant set device name to null");

            string name = (string)jObject["name"];
            if (true)
            {
                return this.GetNameplate();
            }
            else
            {
                return this.BadRequest();
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns>ServiceConfig</returns>
        [HttpGet, Route("config")]
        public IHttpActionResult GetConfig()
        {
            return this.Ok(Service.Config);
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="jObject">配置对象</param>
        /// <returns></returns>
        [HttpPut, Route("config")]
        public IHttpActionResult PutConfig([FromBody] ServiceConfig jObject)
        {
            if (jObject == null)
                return this.BadRequest("参数不可为空");

            string error;
            if (!jObject.IsValid(out error))
                return this.BadRequest(error);
            if (jObject.Equals(Service.Config))
                return this.GetConfig();
            Service.SetServiceConfig(jObject);
            return this.GetConfig();
        }

        /// <summary>
        /// 获取设备状态
        /// </summary>
        /// <returns>EnumWorkStatus</returns>
        [HttpGet, Route("status")]
        public IHttpActionResult GetStatus()
        {
            return this.Ok(Service.Status.ToString());
        }

        /// <summary>
        /// 获取设备时间
        /// </summary>
        /// <returns>DateTime</returns>
        [HttpGet, Route("time")]
        public IHttpActionResult GetTime()
        {
            if (Service.Status == EnumWorkStatus.None || Service.Status == EnumWorkStatus.Disconnected)
                return this.BadRequest();

            return this.Ok(Service.GetDeviceTime());
        }

        /// <summary>
        /// 设置设备时间
        /// </summary>
        /// <param name="jObject">时间字符串</param>
        /// <returns>DateTime</returns>
        [HttpPut, Route("time")]
        public IHttpActionResult PutTime([FromBody] JObject jObject)
        {
            if (Service.Status == EnumWorkStatus.None || Service.Status == EnumWorkStatus.Disconnected
                || jObject == null || jObject["targetTime"] == null)
                return this.BadRequest();

            if (Service.SetDeviceTime((DateTime)jObject["targetTime"]))
                return this.Ok(Service.GetDeviceTime());

            return this.BadRequest();
        }

        /// <summary>
        /// 获取错误信息
        /// 未完成
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("error")]
        public IHttpActionResult GetError()
        {
            return this.Ok();
        }

        /// <summary>
        /// 获取剂量率记录
        /// </summary>
        /// <param name="restrict">after和before条件依据</param>
        /// <param name="after">只返回指定时间“之后“的记录</param>
        /// <param name="includeAfter">是否包含创建时间等于after时刻的记录</param>
        /// <param name="before">只返回指定时间“之前”的记录</param>
        /// <param name="includeBefore">是否包含创建时间等于before时刻的记录</param>
        /// <param name="skip">限定返回的记录跳过的条数</param>
        /// <param name="limit">限定返回的记录条数</param>
        /// <param name="order">指定排序方向，0为逆序，1为正序，按创建时间</param>
        /// <returns></returns>
        [HttpGet, Route("records/dashboard")]
        public IHttpActionResult GetDashboardRecords([FromUri] string restrict = "CreateAt", [FromUri] DateTime? after = null, [FromUri] bool includeAfter = false,
             [FromUri] DateTime? before = null, [FromUri] bool includeBefore = false, [FromUri] uint skip = 0, [FromUri] uint limit = 10,
            [FromUri] string order = null)
        {
            return this.Ok(ObjectStore.Instance.GetObjects<Dashboard>(restrict, after, before, includeAfter, includeBefore, skip, limit, order));
        }

        /// <summary>
        /// 根据ID获取剂量率
        /// </summary>
        /// <param name="recordId">ID</param>
        /// <returns>DoseRateRecord</returns>
        [HttpGet, Route("records/dashboard/{recordId}")]
        public IHttpActionResult GetDashboardRecord(string recordId)
        {
            var result = ObjectStore.Instance.GetObjects<Dashboard>(recordId);
            return result == null ? (IHttpActionResult)this.NotFound() : this.Ok(result);
        }

        /// <summary>
        /// 获取实时剂量率
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("dashboard")]
        public IHttpActionResult GetDashboard()
        {
            var result = Service.GetDashboard();
            if (result == null)
                return this.NotFound();
            return this.Ok(result);
        }

        [HttpGet, Route("records/config")]
        public IHttpActionResult GetConfigRecords([FromUri] string restrict = "CreateAt", [FromUri] DateTime? after = null, [FromUri] bool includeAfter = false,
             [FromUri] DateTime? before = null, [FromUri] bool includeBefore = false, [FromUri] uint skip = 0, [FromUri] uint limit = 10,
            [FromUri] string order = null)
        {
            return this.Ok(ObjectStore.Instance.GetObjects<ServiceConfig>(restrict, after, before, includeAfter, includeBefore, skip, limit, order));
        }
    }
}