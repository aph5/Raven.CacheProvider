using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Web.Caching;
using Newtonsoft.Json;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Json.Linq;

namespace Raven.CacheProvider
{
    public class RavenInMemoryCacheProvider : OutputCacheProvider
    {
        private readonly IDocumentStore _documentStore;

        public RavenInMemoryCacheProvider()
        {
            _documentStore = new EmbeddableDocumentStore {RunInMemory = true};
            _documentStore.Initialize();
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            Debug.WriteLine("Cache.Add(" + key + ", " + entry + ", " + utcExpiry + ")");
            using (var session = _documentStore.OpenSession())
            {
                key = Md5(key);
                var cacheItem = session.Load<CacheItem>(key);
                if (cacheItem != null)
                {
                    if (cacheItem.Expiry.ToUniversalTime() <= DateTime.UtcNow)
                    {
                        session.Delete(cacheItem);
                        session.Advanced.DatabaseCommands.DeleteAttachment(key, null);
                        session.SaveChanges();
                    }
                    else
                        return SerializationHelper.Deserialize(session.Advanced.DatabaseCommands.GetAttachment(key).Data);
                }

                session.Advanced.DatabaseCommands.PutAttachment(key, null, SerializationHelper.Serialize(entry), new RavenJObject());
                session.Store(new CacheItem(key, utcExpiry));
                session.SaveChanges();
            }

            return entry;
        }

        public override object Get(string key)
        {
            Debug.WriteLine("Cache.Get(" + key + ")");
            using (var session = _documentStore.OpenSession())
            {
                key = Md5(key);  
                var cacheItem = session.Load<CacheItem>(key);
                if(cacheItem != null)
                {
                    if (cacheItem.Expiry <= DateTime.UtcNow)
                    {
                        session.Delete(cacheItem);
                        session.Advanced.DatabaseCommands.DeleteAttachment(key, null);
                        session.SaveChanges();
                    }
                    else
                        return SerializationHelper.Deserialize(session.Advanced.DatabaseCommands.GetAttachment(key).Data);
                }

                return null;
            }
        }

        public override void Remove(string key)
        {
            Debug.WriteLine("Cache.Remove(" + key + ")");
            using (var session = _documentStore.OpenSession())
            {
                key = Md5(key);  
                var cacheItem = session.Load<CacheItem>(key);
                if (cacheItem != null)
                {
                    session.Delete(cacheItem);
                    session.Advanced.DatabaseCommands.DeleteAttachment(key, null);
                    session.SaveChanges();
                }
            }
        }

        public override void Set(string key, object entry, DateTime utcExpiry)
        {
            Debug.WriteLine("Cache.Set(" + key + ", " + entry + ", " + utcExpiry + ")");
            using (var session = _documentStore.OpenSession())
            {
                key = Md5(key);  
                var cacheItem = session.Load<CacheItem>(key);
                if (cacheItem != null)
                {
                    session.Advanced.DatabaseCommands.DeleteAttachment(key, null);
                    session.Advanced.DatabaseCommands.PutAttachment(key, null, SerializationHelper.Serialize(entry), new RavenJObject());
                    cacheItem.Expiry = utcExpiry;
                }
                else
                {
                    cacheItem = new CacheItem(key, utcExpiry);
                    session.Advanced.DatabaseCommands.PutAttachment(key, null, SerializationHelper.Serialize(entry), new RavenJObject());
                }

                session.Store(cacheItem);
                session.SaveChanges();
            }
        }

        private static string Md5(string s)
        {
            var provider = new MD5CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(s);
            var builder = new StringBuilder();

            bytes = provider.ComputeHash(bytes);

            foreach (var b in bytes)
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }
}