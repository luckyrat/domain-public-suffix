# domain-public-suffix

.Net library to allow domain names to be analysed according to http://publicsuffix.org rules

70+ Tests also demonstrate how to use the library but in brief:

    DomainName domain;
    // Set the file name for the public suffix rules cache:
    TLDRulesCache.Init(@"C:\temp\publicSuffixDomainCache.txt");
    // Apply the rules to a hostname:
    DomainName.TryParse("tutorial.keefox.org", out domain);
    // Access the top level domain, registrable domain, etc.
    // domain.TLD
    // domain.RegistrableDomain

## Build requirements
* Probably easiest with a recent version of Visual Studio (tested in 2013 community edition)

## Use requirements
* .NET2+
* Write access to user's local folder (~100KB of space)
* Regular (but not constant) internet connectivity to http://publicsuffix.org
    * Could be worked around by hacking the expiry timecode in the cached copy but best not to go there!

## Limitations

(Pull requests welcome)

* No Nuget package yet (I've not investigated how to do that yet)
* Behaviour for hostnames that are not included on the list is probably a bit wrong
* Hostnames constructed from Punycode don't match their equivelant rule
* Wildcard matching needs a little work to match the spec 100% (although there are currently no rules which actually cause a problem)
* Configuration and logging need some work if the library is going to be easily used in any project

Some parts of the data structure and match algorithm are taken from https://github.com/danesparza/domainname-parser under MIT license.
