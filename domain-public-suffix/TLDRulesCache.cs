using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace DomainPublicSuffix
{
    /// <summary>
    /// Holds the full set of rules in memory after loading from either
    /// a local disk cache or from the internet.
    /// </summary>
    public sealed class TLDRulesCache
    {
        private static volatile TLDRulesCache _uniqueInstance;
        private static object _syncObj = new object();
        private static object _syncList = new object();
        private IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> _dicTLDRules;
        private static string _suffixRulesFileLocation;
        private static string _suffixRulesURL;

        private TLDRulesCache()
        {
            //  Initialize our internal dictionary
            _dicTLDRules = GetTLDRules();
        }

        /// <summary>
        /// Call this before accessing the Instance singleton. Yuk! Others may
        /// want to switch to an app/web.config settings approach instead but
        /// in the initial use-case for this library (KeePassRPC) we do not
        /// have access to that configuration
        /// </summary>
        /// <param name="fileName"></param>
        public static void Init (string fileName, string url)
        {
            _suffixRulesFileLocation = fileName;
            _suffixRulesURL = url;
        }

        public static void Init(string fileName)
        {
            Init(fileName, "https://publicsuffix.org/list/public_suffix_list.dat");
        }

        /// <summary>
        /// Returns the singleton instance of the class
        /// </summary>
        public static TLDRulesCache Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_syncObj)
                    {
                        if (_uniqueInstance == null)
                            _uniqueInstance = new TLDRulesCache();
                    }
                }
                return (_uniqueInstance);
            }
        }

        /// <summary>
        /// Dictionary of TLD rules
        /// </summary>
        public IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> TLDRuleLists
        {
            get
            {
                if (_dicTLDRules == null)
                {
                    _dicTLDRules = GetTLDRules();
                }
                return _dicTLDRules;
            }
            set
            {
                _dicTLDRules = value;
            }
        }

        /// <summary>
        /// Resets the singleton class and flushes all the cached 
        /// values so they will be re-cached the next time they are requested
        /// </summary>
        public static void Reset()
        {
            lock (_syncObj)
            {
                _uniqueInstance = null;
            }
        }

        /// <summary>
        /// Gets the list of TLD rules from the cache
        /// </summary>
        /// <returns></returns>
        private IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> GetTLDRules()
        {
            try
            {
                var results = new Dictionary<TLDRule.RuleType, IDictionary<string, TLDRule>>();
                var ruleTypes = (TLDRule.RuleType[])Enum.GetValues(typeof(TLDRule.RuleType));

                foreach (var ruleType in ruleTypes)
                {
                    results[ruleType] = new Dictionary<string, TLDRule>(StringComparer.InvariantCultureIgnoreCase);
                }

                var ruleStrings = ReadRulesData();

                foreach (var ruleString in ruleStrings)
                {
                    var result = new TLDRule(ruleString);
                    results[result.Type][result.Name] = result;
                }

                // Return our results
                return results;
            }
            catch (Exception)
            {
                return new Dictionary<TLDRule.RuleType, IDictionary<string, TLDRule>>();
            }
        }

        private IEnumerable<string> ReadRulesData()
        {
            bool diskCacheValid = false;
            if (File.Exists(_suffixRulesFileLocation))
            {
                //  Load the rules from the cached text file
                var lines = File.ReadAllLines(_suffixRulesFileLocation, Encoding.UTF8);

                // The first line defines our cache expiry time
                for (int i=0; i<lines.Length; i++)
                {
                    var line = lines[i];
                    if (i == 0)
                    {
                        long expiryTicks;
                        if (!long.TryParse(line, out expiryTicks) || expiryTicks < DateTime.UtcNow.Ticks)
                        {
                            diskCacheValid = false;
                            break;
                        }
                        else
                        {
                            diskCacheValid = true;
                            continue;
                        }
                    }
                    yield return line;
                }

                // If the disk cache was not valid, we'll go ahead and download the list from the internet
                if (diskCacheValid)
                    yield break;
            }

            // read the files from the web directly and write into the cache file
            // (using this cache file is not optional because we don't want
            // permissions problems on user machines to result in excessive
            // network traffic)
            using (var cacheFile = File.CreateText(_suffixRulesFileLocation))
            {
                WebClient fileReader = new WebClient();
                string publicSuffixURL = _suffixRulesURL;
                using (Stream datFile = fileReader.OpenRead(publicSuffixURL))
                {
                    using (var reader = new StreamReader(datFile))
                    {
                        cacheFile.WriteLine(DateTime.UtcNow.AddMonths(1).Ticks);

                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Skip empty or comment lines
                            if (line.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)
                                || line.Trim().Length == 0)
                                continue;

                            cacheFile.WriteLine(line);
                            yield return line;
                        }
                    }
                }
            }
        }
    }
}
