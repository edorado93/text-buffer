// <copyright file="IEmitStatistics.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace TextBufferCommon
{
    /// <summary>
    /// Interface for emitting statistics by a text buffer. Every implementation of a text
    /// buffer might have their own fun statistics to emit e.g. number of tree nodes, or, number
    /// of pieces and things like that. Expectatio is to have every text buffer support writing
    /// statistics to terminal and a file for plotting later on.
    /// </summary>
    public interface IEmitStatistics
    {
        /// <summary>
        /// Capture the current state of the system.
        /// </summary>
        void CaptureStatistics();

        /// <summary>
        /// Emit the captures statistics to a file.
        /// </summary>
        /// <param name="fielPath">Path for the file where the statistics are supposed to be written.</param>
        void FlushCapturedStatistics(string filePath);

        /// <summary>
        /// Emit the peak statistics captured over the execution of the process.
        /// </summary>
        void PrintPeakStatistics();
    }
}