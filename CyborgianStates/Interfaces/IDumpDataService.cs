using CyborgianStates.Data.Models.Dump;
using CyborgianStates.Enums;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IDumpDataService
    {
        DumpDataStatus Status { get; }
        ImmutableHashSet<DumpRegion> Regions { get; }
        ImmutableHashSet<DumpNation> Nations { get; }

        Task UpdateAsync();
    }
}