// <copyright file="ProcessMetricsTracker.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TextBufferStats
{
    public class SystemStatistics : IEmitStatistics, IDisposable
    {
        /// <summary>
        /// The main dictionary object containing all of the stats data.
        /// </summary>
        private IList<Metric> stats;

        private readonly IList<MetricEnum> statsToCapture;

        private readonly Process currentProcess;

        /// <summary>
        /// Instantiates the system statistics implementation.
        /// </summary>
        public SystemStatistics()
        {
            this.stats = new List<Metric>();
            this.currentProcess = Process.GetCurrentProcess();
            this.statsToCapture = new List<MetricEnum>() { MetricEnum.WORKING_SET, MetricEnum.TOTAL_PROCESSOR_TIME, MetricEnum.HEAP_BYTES };
        }

        public void Dispose()
        {
            this.currentProcess.Dispose();
        }

        /// <summary>
        /// Capture the current state of the system.
        /// </summary>
        public void CaptureStatistics()
        {
            var currentTime = DateTime.UtcNow;
            var capturedMetrics = new List<long>();
            capturedMetrics.Add(this.currentProcess.WorkingSet64);
            capturedMetrics.Add((long)this.currentProcess.TotalProcessorTime.TotalSeconds);
            capturedMetrics.Add(this.GetGCHeapBytes());
            this.stats.Add(new Metric(currentTime, capturedMetrics));
        }

        /// <summary>
        /// Emit the captures statistics to a file.
        /// </summary>
        /// <param name="fielPath">Path for the file where the statistics are supposed to be written.</param>
        public void FlushCapturedStatistics(string filePath)
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine($"Timestamp,{string.Join(',', this.statsToCapture)}");
            foreach (var stat in this.stats)
            {
                writer.WriteLine(stat.AsString());
            }

            File.WriteAllText(filePath, writer.ToString());
        }

        private long GetGCHeapBytes()
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            return gcMemoryInfo.HeapSizeBytes;
        }
    }
}