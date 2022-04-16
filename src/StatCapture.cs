// <copyright file="StatCapture.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace NotAnotherTextEditor
{
    public class StatCapture
    {
        /// <summary>
        /// Represents the epoch at which the data was captured.
        /// </summary>
        private long CaptureTimeEpoch { get; }

        /// <summary>
        /// The captured values for the various metrics.
        /// </summary>
        private IList<long> capturedMetricValues { get; }

        /// <summary>
        /// Constructs the object representing a single capture of a single metric in time.
        /// </summary>
        /// <param name="captureTime">Time at which the value was recorded.</param>
        /// <param name="capturedValues">Values to be recorded.</param>
        public StatCapture(DateTime captureTime, IList<long> capturedValues)
        {
            this.CaptureTimeEpoch = (long) (captureTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
            this.capturedMetricValues = capturedValues;
        }

        public string AsString()
        {
            return $"{CaptureTimeEpoch},{string.Join(',', this.capturedMetricValues)}";
        }
    }
}