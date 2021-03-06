// <copyright file="Line.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

using System.Text;

namespace TextBufferImplementations.ArrayBuffer
{
    /// <summary>
    /// Represents the metadata and content associated with a given line.
    /// </summary>
    public class Line
    {
        private readonly StringBuilder content;

        public Line(string initialContent)
        {
            this.content = new StringBuilder(initialContent);
        }

        public Line(Line otherLine)
        {
            this.content = new StringBuilder(otherLine.GetContent());
        }

        /// <summary>
        /// Getter function to fetch the content of this line.
        /// </summary>
        /// <returns>A string content of this line.</returns>
        public string GetContent()
        {
            return this.content.ToString();
        }

        public void RemoveSubstring(int start, int length)
        {
            this.content.Remove(start, length);
        }

        public bool IsEmpty()
        {
            return this.content.Length == 0;
        }

        public string RemoveStringToTheRight(int cursor)
        {
            var numberOfCharsInCurrentLineToTheRight = this.GetContentLength() - cursor - (cursor == 0 ? 0 : 1);

            if (numberOfCharsInCurrentLineToTheRight <= 0)
            {
                return string.Empty;
            }

            var substringToTheRight = this.content.ToString(cursor + (cursor == 0 ? 0 : 1), numberOfCharsInCurrentLineToTheRight);
            this.RemoveSubstring(cursor + (cursor == 0 ? 0 : 1), numberOfCharsInCurrentLineToTheRight);
            return substringToTheRight;
        }

        public int GetContentLength()
        {
            return this.content.Length;
        }

        public void AddSubstring(int position, string content)
        {
            this.content.Insert(position, content);
        }
    }
}
