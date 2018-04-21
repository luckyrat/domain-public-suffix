using System;
using System.Collections.Generic;
using System.Text;

namespace DomainPublicSuffix
{
    class DomainCache
    {
        private static Dictionary<string, TLDRule> _domains = new Dictionary<string, TLDRule>();

        public static bool TryGet (string domain, out TLDRule rule)
        {
            if (!_domains.ContainsKey(domain))
            {
                rule = null;
                return false;
            }
            else
            {
                rule = _domains[domain];
                return true;
            }
        }

        public static void Set(string domain, TLDRule rule)
        {
            _domains.Add(domain, rule);
        }
    }
}
