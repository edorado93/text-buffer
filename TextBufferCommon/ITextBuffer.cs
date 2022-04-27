// <copyright file="ITextBuffer.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace TextBufferCommon
{
    /// <summary>
    /// Interface for a text buffer. Contains the essential functions that should be supported
    /// for our analysis and comparison. This is not a full-fledged text editor's buffer but a
    /// minimalistic one with some core features supported.
    /// </summary>
    public interface ITextBuffer
    {
        /// <summary>
        /// Loads the contents of the file into memory. The buffer
        /// can choose to load the entire file or parts of a file in
        /// memory. For comparison purposes, the recommendation is to
        /// load the entire file in memory so that we can measure the time
        /// and space occupied by the process of doing so on large files.
        /// </summary>
        /// <param name="filePath"></param>
        void LoadFile(string filePath);

        /// <summary>
        /// Inserts a new string at the current cursor position. If Seek
        /// is not used, default implementation should perform an append operation.
        /// </summary>
        /// <param name="newString">New string that should be added at the current cursor position.</param>
        void Insert(string newString);

        /// <summary>
        /// Takes the cursor to a specific position within the buffer.
        /// </summary>
        /// <param name="position">Position where the cursor should be taken to.</param>
        void Seek(int position);

        /// <summary>
        /// Takes the cursor to the end of the file.
        /// </summary>
        void SeekToEnd();

        /// <summary>
        /// Takes the cursor to the beginning of the file.
        /// </summary>
        void SeekToBegin();

        /// <summary>
        /// Fetch the content of the provided line number.
        /// </summary>
        /// <param name="lineNumber">Line number whose content should be fetched.</param>
        /// <returns>Line content</returns>
        string GetLineContent(int lineNumber);

        /// <summary>
        /// Delete a certain number of characters from front.
        /// </summary>
        /// <param name="numberOfChars">Number of characters to be deleted.</param>
        void Delete(int numberOfChars);

        /// <summary>
        /// Remove a certain number of characters from behind.
        /// </summary>
        /// <param name="numberOfChars">Number of characters to be removed.</param>
        void Backspace(int numberOfChars);

        /// <summary>
        /// Perform an undo operation for only the last action that we took. We are only going to
        /// suupport a single undo operation and nothing more.
        /// </summary>
        void Undo();

        /// <summary>
        /// Returns the current cursor position. Mostly needed for unit-testing and
        /// if this were a real text editor, we would need to display this information
        /// to the user as well.
        /// </summary>
        /// <returns>The current position of the cursor.</returns>
        int GetCursorPosition();

        /// <summary>
        /// Returns the total length of the file i.e. total number of characters.
        /// </summary>
        /// <returns>The total size of the file in terms of number of characters.</returns>
        int GetFileLength();
    }
}