using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface ISqlProvider
    {
        string GetSql(string key);
    }
}
