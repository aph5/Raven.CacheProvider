using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Raven.CacheProvider.Tests
{
    public class EntryObject
    {
        public string Data { get; set; }
    }

    [TestFixture]
    public class Benchmark
    {
        private RavenInMemoryCacheProvider _provider;
        private String[] _keys;
        private TimeSpan _duration;
        private object _entry;

        [SetUp]
        public void Setup()
        {
            _provider = new RavenInMemoryCacheProvider();
            _keys = new[]
                        {
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                        };
            _duration = TimeSpan.FromSeconds(100.00);
            //_entry = new EntryObject {Data = File.OpenText("TestData.txt").ReadToEnd() };
            _entry = File.OpenText("TestData.txt").ReadToEnd();
        }

        [Test]
        public void Add()
        {
            // Test description
            // Adds 10 different keys
            // Update existing keys
            // Test will be repeated for 10000 iterations ( 10 * 1000 )

            var start = DateTime.Now;
            var iteration = 0;
            for (int i = 0; i < 1000; i++)
            {
                foreach (var key in _keys)
                {
                    _provider.Add(key, _entry, DateTime.UtcNow.Add(_duration));
                    iteration++;
                }
            }

            var end = DateTime.Now;
            string result = String.Format("Add - {0} iteations in {1}", iteration, start - end);
            File.WriteAllText("result_add.txt", result);
        }

        [Test]
        public void Get()
        {
            foreach (var key in _keys)
            {
                _provider.Add(key, _entry, DateTime.UtcNow.Add(_duration));
            }

            var start = DateTime.Now;
            var iteration = 0;
            for (int i = 0; i < 1000; i++)
            {
                foreach (var key in _keys)
                {
                    _provider.Get(key);
                    iteration++;
                }
            }

            var end = DateTime.Now;
            string result = String.Format("Get - {0} iteations in {1}", iteration, start - end);
            File.WriteAllText("result_get.txt", result);
        }

        [Test]
        public void Set()
        {
            foreach (var key in _keys)
            {
                _provider.Add(key, _entry, DateTime.UtcNow.Add(_duration));
            }

            var start = DateTime.Now;
            var iteration = 0;
            for (int i = 0; i < 1000; i++)
            {
                foreach (var key in _keys)
                {
                    _provider.Set(key, _entry, DateTime.UtcNow.Add(_duration));
                    iteration++;
                }
            }

            var end = DateTime.Now;
            string result = String.Format("Get - {0} iteations in {1}", iteration, start - end);
            File.WriteAllText("result_set.txt", result);
        }

        [Test]
        public void Remove()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (var key in _keys)
                {
                    _provider.Add(key + i, _entry, DateTime.UtcNow.Add(_duration));
                }
            }
            var start = DateTime.Now;
            var iteration = 0;

            for (int i = 0; i < 1000; i++)
            {
                foreach (var key in _keys)
                {
                    _provider.Remove(key + 1);
                    iteration++;
                }
            }

            var end = DateTime.Now;
            string result = String.Format("Get - {0} iteations in {1}", iteration, start - end);
            File.WriteAllText("result_remove.txt", result);
        }
    }
}
