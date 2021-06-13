// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

namespace Benchmarks
{
    internal class RatioConfig : ManualConfig
    {
        public RatioConfig()
        {
            SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
        }
    }
}
