using System;
using System.Collections.Generic;
using System.Text;

namespace DomainPublicSuffix
{
    /// <summary>
    /// Meta information class for an individual TLD rule
    /// </summary>
    public class TLDRule : IComparable<TLDRule>
    {
        /// <summary>
        /// The rule name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The rule type
        /// </summary>
        public RuleType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Construct a TLDRule based on a single line from
        /// the www.publicsuffix.org list
        /// </summary>
        /// <param name="RuleInfo"></param>
        public TLDRule(string RuleInfo)
        {
            // Publicsuffix.org spec says a wildcard can be contained at any level not
            // just the left-most. As of Apr 2018 there are no examples of such a rule.
            // According to https://github.com/publicsuffix/list/issues/145 it is 
            // highly likely that we will never need to implement support for this and
            // that the offical spec will be changed to match the reality of our
            // current implementation.

            //  Parse the rule and set properties accordingly:
            if (RuleInfo.StartsWith("*", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Type = RuleType.Wildcard;
                this.Name = RuleInfo.Substring(2);
            }
            else if (RuleInfo.StartsWith("!", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Type = RuleType.Exception;
                this.Name = RuleInfo.Substring(1);
            }
            else
            {
                this.Type = RuleType.Normal;
                this.Name = RuleInfo;
            }
        }

        #region IComparable<TLDRule> Members

        public int CompareTo(TLDRule other)
        {
            if (other == null)
                return -1;

            return Name.CompareTo(other.Name);
        }

        #endregion

        #region RuleType enum
        
        /// <summary>
        /// TLD Rule type
        /// </summary>
        public enum RuleType
        {
            /// <summary>
            /// An exception rule, as defined by www.publicsuffix.org
            /// </summary>
            Exception,

            /// <summary>
            /// A wildcard rule, as defined by www.publicsuffix.org
            /// </summary>
            Wildcard,

            /// <summary>
            /// A normal rule
            /// </summary>
            Normal
        }

        #endregion
    }
}
