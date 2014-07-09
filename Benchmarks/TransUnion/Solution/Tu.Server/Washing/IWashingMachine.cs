using System;
using Tu.Imports;

namespace Tu.Washing
{
    public interface IWashingMachine : IDisposable
    {
        void Export();

        ImportResult Import(DateTime runDate);
    }
}