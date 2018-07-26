using PasLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace PasWebApi
{
    /// <summary>Caches the compute of a grammar.</summary>
    /// <remarks>
    /// <para>
    /// Although functions are stateless, they are kept in memory for a while and can
    /// therefore beneficiate from caching.
    /// </para>
    /// <para>
    /// Preliminary measures (in Azure) half the processing time for a simple grammar
    /// with caching.
    /// </para>
    /// </remarks>
    internal static class GrammarCache
    {
        #region Inner Types
        private class CacheRecord
        {
            public CacheRecord(Grammar grammar)
            {
                Grammar = grammar;
                LastAccessed = DateTime.Now;
            }

            public Grammar Grammar { get; }
            public DateTime LastAccessed { get; private set; }

            public void UpdateAccess()
            {
                LastAccessed = DateTime.Now;
            }
        }
        #endregion

        private const int CACHE_LENGTH = 20;
        private const double PURGE_RATIO = .5;
        private const int PURGE_LENGTH = (int)(PURGE_RATIO * CACHE_LENGTH);

        private readonly static IDictionary<string, CacheRecord> _cache =
            new Dictionary<string, CacheRecord>();

        public static Grammar ParseGrammar(string grammarText)
        {
            var grammar = GetCachedGrammar(grammarText);

            if (grammar != null)
            {
                return grammar;
            }
            else
            {
                grammar = MetaGrammar.ParseGrammar(grammarText);

                PushInCache(grammarText, grammar);

                return grammar;
            }
        }

        private static Grammar GetCachedGrammar(string grammarText)
        {
            lock (_cache)
            {
                CacheRecord record;

                if (_cache.TryGetValue(grammarText, out record))
                {
                    record.UpdateAccess();

                    return record.Grammar;
                }
                else
                {
                    return null;
                }
            }
        }

        private static void PushInCache(string grammarText, Grammar grammar)
        {
            var record = new CacheRecord(grammar);

            lock (_cache)
            {
                if (_cache.Count >= CACHE_LENGTH)
                {
                    PurgeCache();
                }

                _cache[grammarText] = record;
            }
        }

        private static void PurgeCache()
        {
            lock (_cache)
            {
                var keysToPurge = (from pair in _cache
                                   orderby pair.Value.LastAccessed
                                   select pair.Key).Take(PURGE_LENGTH).ToList();

                foreach(var key in keysToPurge)
                {
                    _cache.Remove(key);
                }
            }
        }
    }
}