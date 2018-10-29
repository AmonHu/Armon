using Newtonsoft.Json;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Hql.Ast.ANTLR.Tree;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using Armon.Microservice.Local;
using Armon.Microservice.Model;
using Armon.Microservice.SelfHost;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Configuration = NHibernate.Cfg.Configuration;

namespace Armon.Microservice.Local
{
    /// <summary>
    /// 缓存类 最多缓存20条数据
    /// 注意 微服务涉及到要用时间查询的 一般都是用 starttime这个字段。如果记录创建时间 createAt和starttime一致 则用任意字段都可以
    /// </summary>
    internal class ObjectStore
    {
        private static readonly ObjectStore _instance = new ObjectStore();

        public const int MaxCacheCount = 20;
        private readonly Regex _orderRegex = new Regex(@"(?<order>[-+])?(?<name>[\w]+)[ ,]?");
        private log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ObjectStore));

        public ObjectCache<Dashboard> DashboardCache = new ObjectCache<Dashboard>("CreateAt");

        private ObjectStore()
        {
        }

        public static ObjectStore Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// 初始化缓存
        /// </summary>
        public void Initalize()
        {
        }

        public IList<T> GetObjects<T>(string restrict = "CreateAt", DateTime? after = null, DateTime? before = null,
            bool includeAfter = false, bool includeBefore = false, uint skip = 0, uint limit = 10, string order = "-CreateAt") where T : IEntity
        {
            using (ISession session = this.OpenSession())
            {
                ICriteria crt = session.CreateCriteria(typeof(T));
                if (after != null)
                {
                    crt.Add(includeAfter ? Restrictions.Ge(restrict, after) : Restrictions.Gt(restrict, after));
                }
                if (before != null)
                {
                    crt.Add(includeBefore ? Restrictions.Le(restrict, before) : Restrictions.Lt(restrict, before));
                }

                var orders = ParseOrders(order);
                if (orders.Count > 0)
                {
                    foreach (var each in orders)
                    {
                        crt.AddOrder(each);
                    }
                }
                else
                {
                    crt.AddOrder(new Order(restrict, false));
                }

                crt.SetFirstResult((int)skip).SetMaxResults((int)limit);
                return crt.List<T>();
            }
        }
		
		public bool Clear<T>()
        {
            using (ISession session = this.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.Delete(string.Format("from {0} e", typeof(T).Name));
                session.Flush();
                transaction.Commit();
            }

            return true;
        }

        public bool InsertObject<T>(T @object, bool truncateMillSecond = true) where T : IEntity
        {
            if (@object.RecordId == Guid.Empty) @object.RecordId = Guid.NewGuid();
            if (@object.CreateAt == DateTime.MinValue) @object.CreateAt = DateTime.Now;

            if (truncateMillSecond)
            {
                @object.CreateAt = @object.CreateAt.Truncate(TimeSpan.FromSeconds(1));
            }

            using (ISession session = this.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var returnObject = session.Save(@object);
                session.Flush();
                transaction.Commit();
            }

            return true;
        }

        public T GetObject<T>(string objectId) where T : IEntity
        {
            if (string.IsNullOrEmpty(objectId)) return default(T);

            using (ISession session = this.OpenSession())
            {
                return session.Get<T>(new Guid(objectId));
            }
        }

        private IList<Order> ParseOrders(string strOrder)
        {
            List<Order> orders = new List<Order>();

            if (string.IsNullOrEmpty(strOrder)) return orders;

            var matches = _orderRegex.Matches(strOrder);
            foreach (Match each in matches)
            {
                try
                {
                    if (each.Success)
                    {
                        orders.Add(new Order(each.Groups["name"].Value, each.Groups["order"].Value != "-"));
                    }
                }
                catch (Exception err)
                {
                    logger.ErrorFormat("Parse order {1} error:{0}.", err, each.Value);
                }
            }

            return orders;
        }

        #region NHibernate

        private ISessionFactory _sessionFactory;

        private ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    var configuration = new Configuration().Configure();

                    CreateDBDirectoryIfNeed(configuration);

                    _sessionFactory = configuration.BuildSessionFactory();
                    SchemaMetadataUpdater.QuoteTableAndColumns(configuration);
                    new SchemaUpdate(configuration).Execute(false, true);
                }

                return _sessionFactory;
            }
        }

        internal ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        private void CreateDBDirectoryIfNeed(Configuration configuration)
        {
            var values = configuration.GetProperty("connection.connection_string").Split('=', ';');
            if (values.Length >= 2 && values.ToList().IndexOf("Data Source") >= 0)
            {
                var file = values[values.ToList().IndexOf("Data Source") + 1];
                if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(Path.GetDirectoryName(file)) &&
                    !Directory.Exists(Path.GetDirectoryName(file)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                }
            }
        }

        private bool ExistInSQLiteMaster(string type, string name)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                return session.CreateSQLQuery(string.Format("Select Count(RowID) from SQLITE_MASTER where type='{0}' and name='{1}'", type, name)).UniqueResult<long>() > 0;
            }
        }

        private void CreateIndexAndTriggerIfNeed()
        {
            return;
            List<string> sqlList = new List<string>();

            var tables = GetTableNames();
            foreach (var table in tables)
            {
                if (!IsIndexCreated(string.Format("Index_PK_{0}", table)))
                {
                    sqlList.Add(string.Format("CREATE UNIQUE INDEX Index_PK_{0} ON {0}(RecordId)", table));
                }

                if (!IsIndexCreated(string.Format("Index_CreateAt_{0}", table)))
                {
                    sqlList.Add(string.Format("CREATE INDEX Index_CreateAt_{0} ON {0}(CreateAt)", table));
                }

                if (!IsTriggleCreated(string.Format("Trigger_MaxCount_{0}", table)))
                {
                    sqlList.Add(string.Format(@"CREATE TRIGGER Trigger_MaxCount_{0} AFTER INSERT
ON {0}
WHEN (select count(RowID) from {0} )  > {1}
BEGIN
    DELETE FROM {0} WHERE RecordId IN (SELECT RecordId FROM {0} order by CreateAt LIMIT 10);
END;", table, 100));
                }
            }

            if (sqlList.Any())
            {
                ExecuteSQL(string.Join(";", sqlList));
            }
        }

        private bool IsIndexCreated(string indexName)
        {
            return ExistInSQLiteMaster("index", indexName);
        }

        private bool IsTriggleCreated(string triggerName)
        {
            return ExistInSQLiteMaster("trigger", triggerName);
        }

        private IList<string> GetTableNames()
        {
            using (var currentSession = _sessionFactory.OpenSession())
            {
                return
                    currentSession.CreateSQLQuery("Select name from SQLITE_MASTER where type='table'").List<string>();
            }
        }

        private IList<string> GetTriggers()
        {
            using (var currentSession = _sessionFactory.OpenSession())
            {
                return
                    currentSession.CreateSQLQuery("Select name from SQLITE_MASTER where type='trigger'").List<string>();
            }
        }

        private void DeleteAllTrigger()
        {
            var triggers = GetTriggers();
            if (triggers == null || triggers.Count == 0) return;

            ExecuteSQL(string.Join(";", triggers.Select(e => string.Format("DROP TRIGGER {0}", e))));
        }

        private bool ExecuteSQL(string sql)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                return session.CreateSQLQuery(sql).UniqueResult<int>() > 0;
            }
        }

        #endregion NHibernate
    }

    public class ObjectCache<T> where T : IEntity
    {
        public ObjectCache()
        {
            //Initialize();
        }

        public ObjectCache(string timeProperty)
        {
            _timeProperty = timeProperty;

            //Initialize();
        }

        private ObjectStore Store
        {
            get
            {
                return ObjectStore.Instance;
            }
        }

        private readonly Queue<T> _cache = new Queue<T>();
        private readonly string _timeProperty = "CreateAt";

        private void Initialize()
        {
            var query = GetRecordsFromDB(before: DateTime.Now, includeBefore: true, limit: ObjectStore.MaxCacheCount);
            foreach (var each in query)
            {
                _cache.Enqueue(each);
            }
        }

        public IEnumerable<T> GetCache()
        {
            return _cache;
        }

        public IEnumerable<T> GetObjects(DateTime? after = null, DateTime? before = null,
            bool includeAfter = false, bool includeBefore = false, uint skip = 0, uint limit = 10, int order = 0)
        {
            if (_cache.Count == 0) //没有缓存则直接从数据库中获取
            {
                return GetRecordsFromDB(after, before, includeAfter, includeBefore, skip, limit, order);
            }

            DateTime min = _cache.ToArray().Min(e => e.CreateAt); //获取缓存最小时间
            DateTime max = _cache.ToArray().Max(e => e.CreateAt); //获取缓存最大时间

            if (after != null && before != null && after >= min && before <= max)
            {
                if (order == 0) //逆序
                {
                    return _cache.ToArray()
                        .Where(e => (includeAfter ? (e.GetTime() >= after) : (e.GetTime() > after)) &&
                                    (includeBefore ? (e.GetTime() <= before) : (e.GetTime() < before)))
                        .OrderByDescending(e => e.GetTime()).ThenByDescending(e => e.CreateAt).Skip((int)skip).Take((int)limit);
                }
                else //正序
                {
                    return _cache.ToArray()
                        .Where(e => (includeAfter ? (e.GetTime() >= after) : (e.GetTime() > after)) &&
                                    (includeBefore ? (e.GetTime() <= before) : (e.GetTime() < before)))
                        .OrderBy(e => e.GetTime()).ThenBy(e => e.CreateAt).Skip((int)skip).Take((int)limit);
                }
            }

            if (after != null && after >= min)
            {
                if (order == 0) //逆序
                {
                    return _cache.ToArray()
                        .Where(e => (includeAfter ? (e.GetTime() >= after) : (e.GetTime() > after)))
                        .OrderByDescending(e => e.GetTime()).ThenByDescending(e => e.CreateAt).Skip((int)skip).Take((int)limit);
                }
                else //正序
                {
                    return _cache.ToArray()
                        .Where(e => (includeAfter ? (e.GetTime() >= after) : (e.GetTime() > after)))
                        .OrderBy(e => e.GetTime()).ThenBy(e => e.CreateAt).Skip((int)skip).Take((int)limit);
                }
            }

            if (after == null && before == null && limit + skip <= _cache.Count && order == 0)
            {
                return _cache.ToArray()
                    .OrderByDescending(e => e.GetTime())
                    .ThenByDescending(e => e.CreateAt)
                    .Take((int)limit);
            }

            //直接从数据库中获取
            return GetRecordsFromDB(after, before, includeAfter, includeBefore, skip, limit, order);
        }

        private IEnumerable<T> GetRecordsFromDB(DateTime? after = null, DateTime? before = null, bool includeAfter = false, bool includeBefore = false, uint skip = 0, uint limit = 10, int order = 0)
        {
            return Store.GetObjects<T>(restrict: _timeProperty, after: after, before: before, includeAfter: includeBefore,
                includeBefore: includeAfter, skip: skip, limit: limit, order: order == 1 ? string.Format("+{0}", _timeProperty) : string.Format("-{0}", _timeProperty));
        }

        public T GetObject(string objectId)
        {
            var query = _cache.ToArray().FirstOrDefault(e => e.RecordId.ToString() == objectId);
            if (query != null) return query;

            using (ISession session = Store.OpenSession())
            {
                return session.Get<T>(new Guid(objectId));
            }
        }

        public bool InsertObject(T @object)
        {
            if (Store.InsertObject(@object))
            {
                _cache.Enqueue(@object);
                if (_cache.Count > ObjectStore.MaxCacheCount)
                    _cache.Dequeue();

                return true;
            }

            return false;
        }

        public bool InsertObjects(IEnumerable<T> @objects)
        {
            using (ISession session = Store.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                foreach (var each in objects)
                {
                    each.CreateAt = each.CreateAt.Truncate(TimeSpan.FromSeconds(1));
                    session.Save(each);
                }
                session.Flush();
                transaction.Commit();
            }

            foreach (var each in objects)
            {
                _cache.Enqueue(each);
                if (_cache.Count > ObjectStore.MaxCacheCount)
                    _cache.Dequeue();
            }

            return true;
        }

        public bool UpdateObject(T @object)
        {
            using (ISession session = Store.OpenSession())
            {
                session.Update(@object);
                session.Flush();
            }

            return true;
        }
    }
}