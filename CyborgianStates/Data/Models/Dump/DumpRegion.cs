using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CyborgianStates.Data.Models.Dump
{
    public class DumpRegion
    {
        public string Name { get; set; }
        public string Founder { get; set; }
        public string Delegate { get; set; }
        public HashSet<string> NationNames { get; set; }
        public ConcurrentBag<DumpNation> Nations { get; set; }
    }
}