# domain-public-suffix

.Net library to allow domain names to be analysed according to http://publicsuffix.org rules

90+ tests also demonstrate how to use the library but in brief:

    DomainName domain;
	
    // Set the file name for the public suffix rules cache
    TLDRulesCache.Init(@"C:\temp\publicSuffixDomainCache.txt");
		
    // Apply the rules to a hostname or domain string
    DomainName.TryParse("www.kee.pm", out domain);
	
    // Access the top level domain, registrable domain, etc.
    // domain.TLD == "pm"
    // domain.RegistrableDomain == "kee.pm"

## Build requirements
* None really, but is tested in Visual Studio 2017 Community Edition

## Use requirements
* .NET2+
* Write access to user's local folder (~100KB of space)
* Regular (but not constant) internet connectivity to http://publicsuffix.org
    * Could be worked around by hacking the expiry timecode in the cached copy but best not to go there!

## Limitations

(Pull requests welcome)

* No Nuget package yet (I've not investigated how to do that yet)
* Configuration and logging need some work if the library is going to be easily used in any project

Some parts of the data structure and match algorithm are taken from https://github.com/danesparza/domainname-parser under MIT license.
