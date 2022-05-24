// <copyright file="ArrayBuffer.cs" company="PeaceMaker">
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
            var charactersToBeRemoved = numberOfChars;

            while (this.lineCursor >= 0 && numberOfChars > 0)
            {
                var currentLine = this.lineNumberToLineMap[this.lineCursor];

                // If we are at the end of the current line, we don't need any additional increments. Else, we add one to represent
                // the number of characters available for deletion on the current line.
                var charsOnCurrentLineForBackspace = this.lineTextCursor + (this.IsCursorOnLineEnd() ? 0 : 1);

                // All the deletions are going to happen from the current line only!
                if (numberOfChars <= charsOnCurrentLineForBackspace)
                {
                    currentLine.RemoveSubstring(this.lineTextCursor - numberOfChars + (this.IsCursorOnLineEnd() ? 0 : 1), numberOfChars);
                    this.lineTextCursor -= numberOfChars;

                    if (currentLine.IsEmpty())
                    {
                        this.RemoveLine(this.lineCursor);
                    }

                    numberOfChars = 0;
                }
                else
                {
                    numberOfChars -= charsOnCurrentLineForBackspace;

                    // Remove all the characters from the current line
                    // starting from the cursor position within that line.
                    currentLine.RemoveSubstring(0, charsOnCurrentLineForBackspace);

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

            this.totalFileLength -= charactersToBeRemoved;
        }

        /// <inheritdoc/>
        public void Delete(int numberOfChars)
        {
            // Save for undo.
            this.SaveBufferState();
            var charactersToBeRemoved = numberOfChars;

            while (this.lineCursor < this.file.Count && numberOfChars > 0)
            {
                var currentLine = this.lineNumberToLineMap[this.lineCursor];
                var numberOfCharsInCurrentLineToTheRight = currentLine.GetContentLength() - this.lineTextCursor - (this.lineTextCursor == 0 ? 0 : 1);

                // All the deletions are going to happen from the current line only!
                // Note that the line's text cursor does not move in this case since it
                // represents number of characters from the beginning (positon wise) and
                // that does not change during deletion.
                if (numberOfChars <= numberOfCharsInCurrentLineToTheRight)
                {
                    currentLine.RemoveSubstring(this.lineTextCursor + (this.lineTextCursor == 0 ? 0 : 1), numberOfChars);
                    numberOfChars = 0;

                    if (currentLine.IsEmpty())
                    {
                        this.RemoveLine(this.lineCursor);
                    }
                }
                else
                {
                    numberOfChars -= numberOfCharsInCurrentLineToTheRight;

                    // Remove all the characters from the current line
                    // starting from the cursor position within that line.
                    currentLine.RemoveSubstring(this.lineTextCursor + (this.lineTextCursor == 0 ? 0 : 1), numberOfCharsInCurrentLineToTheRight);

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
            }

            this.totalFileLength -= charactersToBeRemoved;
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
            if (string.IsNullOrEmpty(newString))
            {
                return;
            }

            // Save for undo.
            this.SaveBufferState();

            var lines = newString.Split(Environment.NewLine);
            var lastLineLength = lines.Last().Length;

            if (this.IsBufferEmpty())
            {
                foreach (var updatedLine in lines)
                {
                    var lineObj = new Line(updatedLine);
                    this.file.Add(lineObj);
                    this.lineNumberToLineMap[0] = lineObj;
                }
            }
            else
            {
                if (lines.Count() == 1)
                {
                    this.lineNumberToLineMap[this.lineCursor].AddSubstring(this.lineTextCursor + (this.IsCursorOnLineEnd() ? 0 : 1), lines[0]);
                }
                else
                {
                    var lastLine = this.lineNumberToLineMap[this.lineCursor].RemoveStringToTheRight(this.lineTextCursor);
                    this.lineNumberToLineMap[this.lineCursor].AddSubstring(this.lineTextCursor + (this.IsCursorOnLineEnd() ? 0 : 1), lines[0]);
                    lines[lines.Length - 1] = lines.Last() + lastLine;
                    this.AddLinesAfter(this.lineCursor, lines.Skip(1));
                }
            }

            this.totalFileLength += newString.Length - (lines.Length - 1) * Environment.NewLine.Length;
            this.lineTextCursor = lastLineLength;
            this.lineCursor += lines.Length - 1;
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

            if (this.IsBufferEmpty())
            {
                this.lineCursor = 0;
                this.lineTextCursor = 0;
            }
            else
            {
                this.lineCursor--;
                this.lineTextCursor = this.file.Last().GetContentLength();
            }
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
                position = this.totalFileLength;
            }

            int totalCharactersConsidered = 0, lineNumber = 0;
            foreach (var line in this.file)
            {
                var lineLength = line.GetContentLength();

                // That means we have found the line containing the seek position.
                if (totalCharactersConsidered + lineLength >= position)
                {
                    this.lineCursor = lineNumber;
                    this.lineTextCursor = (position - totalCharactersConsidered);
                    break;
                }

                totalCharactersConsidered += lineLength;
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

        private void AddLinesAfter(int lineNumber, IEnumerable<string> newLines)
        {
            // Update remaining mappings.
            for (int idx = this.file.Count - 1; idx > lineNumber; idx--)
            {
                this.lineNumberToLineMap[idx + newLines.Count()] = this.lineNumberToLineMap[idx];
            }

            for (int idx = 0; idx < newLines.Count(); idx++)
            {
                var newLine = new Line(newLines.ElementAt(idx));
                this.file.Insert(lineNumber + idx + 1, newLine);
                this.lineNumberToLineMap[lineNumber + idx + 1] = newLine;
            }
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
                this.File = new List<Line>();
                this.LineNumberToLineMap = new Dictionary<int, Line>();
                foreach (var lineNumber in lineNumberToLineMap.Keys)
                {
                    var lineClone = new Line(lineNumberToLineMap[lineNumber]);
                    this.File.Add(lineClone);
                    this.LineNumberToLineMap[lineNumber] = lineClone;
                }

                this.LineCursor = lineCursor;
                this.LineTextCursor = lineTextCursor;
                this.TotalFileLength = totalFileLength;
                this.CanRestore = true;
            }
        }

        private bool IsBufferEmpty()
        {
            return this.file.Count == 0;
        }

        private bool IsCursorOnLineEnd()
        {
            return this.lineTextCursor == this.lineNumberToLineMap[this.lineCursor].GetContentLength();
        }
    }
}
