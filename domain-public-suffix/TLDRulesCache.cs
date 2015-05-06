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
        private List<TLDRule> _lstTLDRules;
        private static string _suffixRulesFileLocation;

        private TLDRulesCache()
        {
            //  Initialize our internal list:
            _lstTLDRules = GetTLDRules();
            _lstTLDRules.Sort();
        }

        /// <summary>
        /// Call this before accessing the Instance singleton. Yuk! Others may
        /// want to switch to an app/web.config settings approach instead but
        /// in the initial use-case for this library (KeePassRPC) we do not
        /// have access to that configuration
        /// </summary>
        /// <param name="fileName"></param>
        public static void Init (string fileName)
        {
            _suffixRulesFileLocation = fileName;
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
        /// List of TLD rules
        /// </summary>
        public List<TLDRule> TLDRuleList
        {
            get
            {
                return _lstTLDRules;
            }
            set
            {
                _lstTLDRules = value;
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
        private List<TLDRule> GetTLDRules()
        {
            try
            {
                var ruleStrings = ReadRulesData();

                List<TLDRule> lstTLDRules = new List<TLDRule>(7000);
                foreach (var ruleString in ruleStrings)
                    lstTLDRules.Add(new TLDRule(ruleString));

                //  Return our results:
                Debug.WriteLine(string.Format("Loaded {0} rules into cache.", lstTLDRules.Count));
                return lstTLDRules;
            }
            catch (Exception)
            {
                return new List<TLDRule>();
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
                string publicSuffixURL = "https://publicsuffix.org/list/effective_tld_names.dat";
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
