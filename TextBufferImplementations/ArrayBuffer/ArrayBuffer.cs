﻿// <copyright file="ArrayBuffer.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

using TextBufferCommon;
using TextBufferImplementations.ArrayBuffer.Exceptions;

namespace TextBufferImplementations.ArrayBuffer
{
    /// <summary>
    /// An array of strings implementation of the text buffer.
    /// We keep every line from the file as a line in the buffer and
    /// the entire file becomes a list of lines.
    /// </summary>
    public class ArrayBuffer : ITextBuffer
    {
        private readonly IFile fileWrapper;
        private List<Line> file = new List<Line>();
        private Dictionary<int, Line> lineNumberToLineMap = new Dictionary<int, Line>();
        private InternalState previousState = new InternalState();
        private int lineCursor;
        private int lineTextCursor;
        private int totalFileLength;

        /// <summary>
        /// Instantiates an object of the class.
        /// </summary>
        /// <param name="fileWrapper">A wrapper object used for interacting with files.</param>
        public ArrayBuffer(IFile fileWrapper)
        {
            this.fileWrapper = fileWrapper;
        }

        /// <inheritdoc/>
        public void Backspace(int numberOfChars)
        {
            // Save for undo.
            this.SaveBufferState();

            while (this.lineCursor >= 0 && numberOfChars > 0)
            {
                var currentLine = this.lineNumberToLineMap[this.lineCursor];

                // All the deletions are going to happen from the current line only!
                if (numberOfChars <= this.lineTextCursor)
                {
                    this.lineTextCursor -= numberOfChars;
                    currentLine.RemoveSubstring(this.lineTextCursor - numberOfChars + 1, numberOfChars);
                }
                else
                {
                    numberOfChars -= this.lineTextCursor;

                    // Remove all the characters from the current line
                    // starting from the cursor position within that line.
                    currentLine.RemoveSubstring(0, this.lineTextCursor);

                    // If we removed all characters from the current line,
                    // remove that line from the file.
                    if (currentLine.IsEmpty())
                    {
                        this.RemoveLine(this.lineCursor);
                    }

                    // Move the line cursor one step back.
                    this.lineCursor -= 1;
                    this.lineTextCursor = this.lineNumberToLineMap[this.lineCursor].GetContentLength();
                    
                }
            }

            // We've deleted all the content that was possible to remove from the beginning of the original line cursor.
            if (this.lineCursor == -1)
            {
                this.lineCursor = 0;
            }

            this.totalFileLength -= numberOfChars;
        }

        /// <inheritdoc/>
        public void Delete(int numberOfChars)
        {
            // Save for undo.
            this.SaveBufferState();

            while (this.lineCursor < this.file.Count && numberOfChars > 0)
            {
                var currentLine = this.lineNumberToLineMap[this.lineCursor];
                var numberOfCharsInCurrentLineToTheRight = currentLine.GetContentLength() - this.lineTextCursor;

                // All the deletions are going to happen from the current line only!
                // Note that the line's text cursor does not move in this case since it
                // represents number of characters from the beginning (positon wise) and
                // that does not change during deletion.
                if (numberOfChars <= numberOfCharsInCurrentLineToTheRight)
                {
                    currentLine.RemoveSubstring(this.lineTextCursor, numberOfChars);
                }
                else
                {
                    numberOfChars -= numberOfCharsInCurrentLineToTheRight;

                    // Remove all the characters from the current line
                    // starting from the cursor position within that line.
                    currentLine.RemoveSubstring(this.lineTextCursor, numberOfCharsInCurrentLineToTheRight);

                    // If we removed all characters from the current line,
                    // remove that line from the file.
                    if (currentLine.IsEmpty())
                    {
                        this.RemoveLine(this.lineCursor);
                    }
                    else
                    {
                        this.lineCursor += 1;
                    }

                    this.lineTextCursor = 0;
                }

                this.totalFileLength -= numberOfChars;
            }

            // We've deleted all the content that was possible to remove from the beginning of the original line cursor.
            if (this.lineCursor == -1)
            {
                this.lineCursor = 0;
            }
        }

        /// <inheritdoc/>
        public string GetLineContent(int lineNumber)
        {
            if (this.lineNumberToLineMap.ContainsKey(lineNumber))
            {
                return this.lineNumberToLineMap[lineNumber].GetContent();
            }

            throw new InvalidLineNumberException($"Provided line number does not exist: {lineNumber}");
        }

        /// <inheritdoc/>
        public void Insert(string newString)
        {
            // Save for undo.
            this.SaveBufferState();

            this.lineNumberToLineMap[this.lineCursor].AddSubstring(this.lineTextCursor, newString);
            this.totalFileLength += newString.Length;
            this.lineTextCursor += newString.Length;
        }

        /// <inheritdoc/>
        public void LoadFile(string filePath)
        {
            var lines = this.fileWrapper.ReadLines(filePath);
            var lineNumber = 0;
            foreach (var line in lines)
            {
                var lineObj = new Line(line);

                this.lineCursor++;
                this.totalFileLength += line.Length;

                this.file.Add(lineObj);
                this.lineNumberToLineMap.Add(lineNumber, lineObj);

                lineNumber++;
            }

            this.lineCursor--;
            this.lineTextCursor = this.file.Last().GetContentLength();
        }

        /// <inheritdoc/>
        public void Seek(int position)
        {
            if (position < 0)
            {
                position = 0;
            }
            else if (position > this.totalFileLength)
            {
                position = this.totalFileLength + 1;
            }

            int totalCharactersConsidered = 0, lineNumber = 0;
            foreach (var line in this.file)
            {
                var lineLength = line.GetContentLength();

                // That means we have found the line containing the seek position.
                if (totalCharactersConsidered + lineLength > position)
                {
                    this.lineCursor = lineNumber;
                    this.lineTextCursor = (position - totalCharactersConsidered);
                }

                lineNumber++;
            }
        }

        /// <inheritdoc/>
        public void Undo()
        {
            if (this.previousState.CanRestore)
            {
                this.file = this.previousState.File;
                this.lineNumberToLineMap = this.previousState.LineNumberToLineMap;
                this.lineCursor = this.previousState.LineCursor;
                this.lineTextCursor = this.previousState.LineTextCursor;
                this.totalFileLength = this.previousState.TotalFileLength;

                // We are only supporting a single Undo at this time.
                this.previousState.CanRestore = false;
            }
        }

        /// <inheritdoc/>
        public int GetCursorPosition()
        {
            int cursorPosition = 0;
            for (int lineNumber = 0; lineNumber < this.lineCursor; lineNumber++)
            {
                cursorPosition += this.lineNumberToLineMap[lineNumber].GetContentLength();
            }

            return cursorPosition + this.lineTextCursor;
        }

        /// <inheritdoc/>
        public int GetFileLength()
        {
            return this.totalFileLength;
        }

        /// <inheritdoc/>
        private void RemoveLine(int lineNumber)
        {
            // Update remaining mappings.
            for (int idx = lineNumber + 1; idx < this.file.Count; idx++)
            {
                this.lineNumberToLineMap[idx - 1] = this.lineNumberToLineMap[idx];
            }

            // Remove the last entry from the dictionary.
            this.lineNumberToLineMap.Remove(this.file.Count - 1);

            // Update the file length.
            this.totalFileLength -= this.file[lineNumber].GetContentLength();

            // Remove the line from the list of files. This is an O(n) operation.
            this.file.RemoveAt(lineNumber);
        }

        /// <inheritdoc/>
        public void SeekToEnd()
        {
            this.lineCursor = this.file.Count - 1;
            this.lineTextCursor = this.file.Last().GetContentLength();
        }

        /// <inheritdoc/>
        public void SeekToBegin()
        {
            this.lineCursor = 0;
            this.lineTextCursor = 0;
        }

        private void SaveBufferState()
        {
            this.previousState.SaveCurrentState(this.file, this.lineNumberToLineMap, this.lineCursor, this.lineTextCursor, this.totalFileLength);
        }

        private class InternalState
        {
            public List<Line> File { get; internal set; } = new List<Line>();
            
            public Dictionary<int, Line> LineNumberToLineMap { get; internal set; } = new Dictionary<int, Line>();
            
            public int LineCursor { get; internal set; }
            
            public int LineTextCursor { get; internal set; }
            
            public int TotalFileLength { get; internal set; }

            public bool CanRestore { get; set; }

            public void SaveCurrentState(List<Line> file, Dictionary<int, Line> lineNumberToLineMap, int lineCursor, int lineTextCursor, int totalFileLength)
            {
                this.File = file;
                this.LineNumberToLineMap = lineNumberToLineMap;
                this.LineCursor = lineCursor;
                this.LineTextCursor = lineTextCursor;
                this.TotalFileLength = totalFileLength;
                this.CanRestore = true;
            }
        }
    }
}
