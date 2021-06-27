using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CyborgianStates.Data.Models.Dump
{
    public class DumpNation
    {
        public string Name { get; set; }
        public bool IsWAMember { get; set; }
        public List<string> Endorsements { get; set; }
        private DumpRegion _region;
        public DumpRegion Region
        {
            get => _region;
            set
            {
                _region = value;

                if (_region != null)
                {
                    if (_region.Nations == null)
                    {
                        _region.Nations = new ConcurrentBag<DumpNation>();
                    }
                    _region.Nations.Add(this);
                }
            }
        }
    }
}