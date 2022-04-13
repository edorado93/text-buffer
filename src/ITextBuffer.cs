// <copyright file="ITextBuffer.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

namespace NotAnotherTextEditor
{
    /// <summary>
    /// Interface for a text buffer. Contains the essential functions that should be supported
    /// for our analysis and comparison. This is not a full-fledged text editor's buffer but a
    /// minimalistic one with some core features supported.
    /// </summary>
    public interface ITextBuffer
    {
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
        /// Fetch the content of the provided line number.
        /// </summary>
        /// <param name="lineNumber">Line number whose content should be fetched.</param>
        void GetLineContent(int lineNumber);

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
    }
}