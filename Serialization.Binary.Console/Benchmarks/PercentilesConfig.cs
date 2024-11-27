using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;

namespace Serialization.Binary.Benchmarks;

public class PercentilesConfig: ManualConfig
{
    public PercentilesConfig()
    {
        // AddColumn(StatisticColumn.P50);
        // AddColumn(StatisticColumn.P90);
        AddColumn(StatisticColumn.P95);
        // AddColumn(StatisticColumn.P100);
    }
}