// <copyright file="ArrayBuffer.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

using TextBufferCommon;

namespace TextBufferImplementations.ArrayBuffer
{
    /// <summary>
    /// An array of strings implementation of the text buffer.
    /// We keep every line from the file as a line in the buffer and
    /// the entire file becomes a list of lines.
    /// </summary>
    public class ArrayBuffer : ITextBuffer
    {
        private readonly List<Line> file = new List<Line>();
        private readonly Dictionary<int, Line> lineNumberToLineMap = new Dictionary<int, Line>();
        private int lineCursor;
        private int lineTextCursor;
        private int totalFileLength;
        private readonly FileWrapper fileWrapper;

        /// <summary>
        /// Instantiates an object of the class.
        /// </summary>
        /// <param name="fileWrapper">A wrapper object used for interacting with files.</param>
        public ArrayBuffer(FileWrapper fileWrapper)
        {
            this.fileWrapper = fileWrapper;
        }

        /// <inheritdoc/>
        public void Backspace(int numberOfChars)
        {
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
        }

        /// <inheritdoc/>
        public void Delete(int numberOfChars)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetLineContent(int lineNumber)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Insert(string newString)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void LoadFile(string filePath)
        {
            var lines = this.fileWrapper.ReadLines(filePath);
            var lineNumber = 0;
            foreach (var line in lines)
            {
                var lineObj = new Line(this.lineCursor, line);

                this.lineCursor++;
                this.totalFileLength += line.Length;

                this.file.Add(lineObj);
                this.lineNumberToLineMap.Add(lineNumber, lineObj);

                lineNumber++;
            }

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
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int GetCursorPosition()
        {
            return 0;
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
    }
}
