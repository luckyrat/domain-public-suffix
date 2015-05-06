using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DomainPublicSuffix
{
    public class DomainName
    {
        #region Private members

        private string _subDomain = string.Empty;
        private string _domain = string.Empty;
        private string _tld = string.Empty;
        private TLDRule _tldRule = null;

        #endregion

        #region Public properties

        /// <summary>
        /// The subdomain portion
        /// </summary>
        public string SubDomain
        {
            get
            {
                return _subDomain;
            }
        }

        /// <summary>
        /// The domain name portion, without the subdomain or the TLD
        /// </summary>
        public string Domain
        {
            get
            {
                return _domain;
            }
        }

        /// <summary>
        /// The domain name portion and TLD, without the subdomain
        /// </summary>
        public string RegistrableDomain
        {
            get
            {
                StringBuilder output = new StringBuilder();
                if (!string.IsNullOrEmpty(_domain))
                    output.Append(_domain).Append(".");
                output.Append(_tld);
                return output.ToString();
            }
        }

        /// <summary>
        /// The entire combined hostname
        /// </summary>
        public string Hostname
        {
            get
            {
                StringBuilder output = new StringBuilder();
                if (!string.IsNullOrEmpty(_subDomain))
                    output.Append(_subDomain).Append(".");
                if (!string.IsNullOrEmpty(_domain))
                    output.Append(_domain).Append(".");
                output.Append(_tld);
                return output.ToString();
            }
        }

        /// <summary>
        /// The TLD portion
        /// </summary>
        public string TLD
        {
            get
            {
                return _tld;
            }
        }

        /// <summary>
        /// The matching TLD rule
        /// </summary>
        public TLDRule TLDRule
        {
            get
            {
                return _tldRule;
            }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Constructs a DomainName object from the string representation of a domain. 
        /// </summary>
        /// <param name="domainString"></param>
        public DomainName(string domainString)
        {
            //  If an exception occurs it should bubble up past this
            ParseDomainName(domainString != null ? domainString.ToLowerInvariant() : domainString
                , out _tld, out _domain, out _subDomain, out _tldRule);
        }

        /// <summary>
        /// Constructs a DomainName object from its 3 parts
        /// </summary>
        /// <param name="TLD">The top-level domain</param>
        /// <param name="SLD">The second-level domain</param>
        /// <param name="SubDomain">The subdomain portion</param>
        /// <param name="TLDRule">The rule used to parse the domain</param>
        private DomainName(string TLD, string SLD, string SubDomain, TLDRule TLDRule)
        {
            this._tld = TLD != null ? TLD.ToLowerInvariant() : TLD;
            this._domain = SLD != null ? SLD.ToLowerInvariant() : SLD;
            this._subDomain = SubDomain != null ? SubDomain.ToLowerInvariant() : SubDomain;
            this._tldRule = TLDRule;
        }

        #endregion

        #region Parse domain

        /// <summary>
        /// Converts the string representation of a domain to it's 3 distinct components: 
        /// Top Level Domain (TLD), Second Level Domain (SLD), and subdomain information
        /// </summary>
        /// <param name="domainString">The domain to parse</param>
        /// <param name="TLD"></param>
        /// <param name="Domain"></param>
        /// <param name="SubDomain"></param>
        /// <param name="MatchingRule"></param>
        private static void ParseDomainName(string domainString, out string TLD, out string Domain, out string SubDomain, out TLDRule MatchingRule)
        {
            TLD = string.Empty;
            Domain = string.Empty;
            SubDomain = string.Empty;
            MatchingRule = null;

            //  If the fqdn is empty, we have a problem already
            if (domainString.Trim() == string.Empty)
                throw new ArgumentException("The domain cannot be blank");

            //  Next, find the matching rule:
            MatchingRule = FindMatchingTLDRule(domainString);

            //  At this point, no rules match, we have a problem
            if(MatchingRule == null)
                throw new FormatException("The domain does not have a recognized TLD");

            //  Based on the tld rule found, get the domain (and possibly the subdomain)
            string tempSudomainAndDomain = string.Empty;
            int tldIndex = 0;
            
            //  First, determine what type of rule we have, and set the TLD accordingly
            switch (MatchingRule.Type)
            {
                case TLDRule.RuleType.Normal:
                    tldIndex = domainString.IndexOf("." + MatchingRule.Name, StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
                case TLDRule.RuleType.Wildcard:
                    //  This finds the last portion of the TLD...
                    tldIndex = domainString.IndexOf("." + MatchingRule.Name, StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);

                    //  But we need to find the wildcard portion of it:
                    tldIndex = tempSudomainAndDomain.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
                case TLDRule.RuleType.Exception:
                    tldIndex = domainString.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
            }

            //  See if we have a subdomain:
            List<string> lstRemainingParts = new List<string>(tempSudomainAndDomain.Split('.'));

            //  If we have 0 parts left, there is just a tld and no domain or subdomain
            //  If we have 1 part, it's the domain, and there is no subdomain
            //  If we have 2+ parts, the last part is the domain, the other parts (combined) are the subdomain
            if (lstRemainingParts.Count > 0)
            {
                //  Set the domain:
                Domain = lstRemainingParts[lstRemainingParts.Count - 1];

                //  Set the subdomain, if there is one to set:
                if (lstRemainingParts.Count > 1)
                {
                    //  We strip off the trailing period, too
                    SubDomain = tempSudomainAndDomain.Substring(0, tempSudomainAndDomain.Length - Domain.Length - 1);
                }
            }
        }

        #endregion

        #region TryParse method(s)

        /// <summary>
        /// Converts the string representation of a domain to its DomainName equivalent.  A return value
        /// indicates whether the operation succeeded.
        /// </summary>
        /// <param name="domainString"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string domainString, out DomainName result)
        {
            //  Our temporary domain parts:
            string _tld = string.Empty;
            string _sld = string.Empty;
            string _subdomain = string.Empty;
            TLDRule _tldrule = null;
            result = null;

            try
            {
                //  Try parsing the domain name ... this might throw formatting exceptions
                ParseDomainName(domainString != null ? domainString.ToLowerInvariant() : domainString,
                    out _tld, out _sld, out _subdomain, out _tldrule);

                //  Construct a new DomainName object and return it
                result = new DomainName(_tld, _sld, _subdomain, _tldrule);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Rule matching
        /// <summary>
        /// Finds matching rule for a domain.  If no rule is found, 
        /// returns null
        /// </summary>
        /// <param name="domainString"></param>
        /// <returns></returns>
        private static TLDRule FindMatchingTLDRule(string domainString)
        {
            TLDRule fromCache = DomainCache.Get(domainString);
            if (fromCache != null)
                return fromCache;

            if (domainString.StartsWith("."))
                return null;

            //  Split our domain into parts (based on the '.')
            //  ...Put these parts in a list
            //  ...Make sure these parts are in reverse order (we'll be checking rules from the right-most part of the domain)
            List<string> lstDomainParts = new List<string>(domainString.Split('.'));
            lstDomainParts.Reverse();

            //  Begin building our partial domain to check rules with:
            string checkAgainst = string.Empty;

            //  Our 'matches' collection:
            List<TLDRule> ruleMatches = new List<TLDRule>();

            foreach (string domainPart in lstDomainParts)
            {
                //  Add on our next domain part:
                checkAgainst = string.Format("{0}.{1}", domainPart, checkAgainst);

                //  If we end in a period, strip it off:
                if (checkAgainst.EndsWith("."))
                    checkAgainst = checkAgainst.Substring(0, checkAgainst.Length - 1);

                //  Try to match an exception rule:
                foreach (var rule in TLDRulesCache.Instance.TLDRuleList)
                {
                    if (rule.Name == checkAgainst
                        && rule.Type == TLDRule.RuleType.Exception)
                    // As soon as we find any exception rule we have to stop and say that is our best match
                    {
                        DomainCache.Set(domainString, rule);
                        return rule;
                    }
                }
                
                //  Try to match a wildcard or normal rule:
                foreach (var rule in TLDRulesCache.Instance.TLDRuleList)
                {
                    if (rule.Name == checkAgainst
                        && (rule.Type == TLDRule.RuleType.Wildcard || rule.Type == TLDRule.RuleType.Normal))
                        ruleMatches.Add(rule);
                }
            }

            //  Find the longest match
            ruleMatches.Sort(new Comparison<TLDRule>(CompareLength));
            TLDRule primaryMatch = ruleMatches[0];
            DomainCache.Set(domainString, primaryMatch);
            return primaryMatch;
        }

        static int CompareLength(TLDRule a, TLDRule b)
        {
            // It's the number of hostname sections that matters, not the total
            // character length (this matters when wildcards are in the mix)
            return b.Name.Split('.').Length.CompareTo(a.Name.Split('.').Length);
        }
        #endregion
    }

}
