// <copyright file="FileWrapper.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace TextBufferCommon
{
    /// <summary>
    /// A wrapper class over the file object. Primarily used for dependency
    /// injection so that the file related functions can be mocked out.
    /// </summary>
    public class FileWrapper : IFile
    {
        /// <summary>
        /// Reads all of the lines contained in the file.
        /// </summary>
        /// <param name="filePath">Absolute path of the file.</param>
        /// <returns></returns>
        public IEnumerable<string> ReadLines(string filePath)
        {
            return File.ReadLines(filePath);
        }
    }
}
