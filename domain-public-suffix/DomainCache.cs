using System;
using System.Collections.Generic;
using System.Text;

namespace DomainPublicSuffix
{
    class DomainCache
    {
        private static Dictionary<string, TLDRule> _domains = new Dictionary<string, TLDRule>();

        public static TLDRule Get (string domain)
        {
            return _domains.ContainsKey(domain) ? _domains[domain] : null;
        }

        public static void Set(string domain, TLDRule rule)
        {
            _domains.Add(domain, rule);
        }
    }
}
