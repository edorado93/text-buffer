// <copyright file="TesterUtils.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace TextBufferTester
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TextBufferCommon;

    /// <summary>
    /// A collection of static utility functions required for testing.
    /// </summary>
    public class TesterUtils
    {
        private readonly static Random randomGenerator = new Random();

        /// <summary>
        /// Creates a mock object representing a file wrapper.
        /// </summary>
        /// <param name="numberOfLines">Number of lines to add in the file</param>
        /// <param name="minLineLength">Each line should contain at least these many characters.</param>
        /// <param name="maxLineLength">Each line can contain a maximum of these many characters.</param>
        /// <returns>Returns a mock object for the file wrapper.</returns>
        public static Mock<IFile> CreateFile(ref IList<string> lines, int numberOfLines = 10, int minLineLength = 5, int maxLineLength = 50)
        {
            int lineNumber = 0;
            while (lineNumber < numberOfLines)
            {
                var nextLineLength = randomGenerator.Next(minLineLength, maxLineLength);
                lines.Add(GetRandomString(nextLineLength, lineNumber));

                lineNumber++;
            }

            var fileWrapperMoq = new Mock<IFile>();
            fileWrapperMoq.Setup(f => f.ReadLines(It.IsAny<string>())).Returns(lines);

            return fileWrapperMoq;
        }

        /// <summary>
        /// Creates a mock object representing a file wrapper.
        /// </summary>
        /// <param name="lines">The lines that are to be added to the file.</param>
        /// <returns>Returns a mock object for the file wrapper.</returns>
        public static Mock<IFile> CreateFileCustom(IEnumerable<string> lines)
        {
            var fileWrapperMoq = new Mock<IFile>();
            fileWrapperMoq.Setup(f => f.ReadLines(It.IsAny<string>())).Returns(lines);
            return fileWrapperMoq;
        }

        private static string GetRandomString(int length, int lineNumber)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var content = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[randomGenerator.Next(s.Length)]).ToArray());
            return $"{lineNumber}-{content}";
        }
    }
}
