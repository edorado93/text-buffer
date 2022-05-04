// <copyright file="IFile.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace TextBufferCommon
{
    public interface IFile
    {
        IEnumerable<string> ReadLines(string filePath);
    }
}
