// <copyright file="InvalidLineNumberException.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

using TextBufferCommon.Exceptions;

namespace TextBufferImplementations.ArrayBuffer.Exceptions
{
    public class InvalidLineNumberException : InvalidBufferOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLineNumberException" /> class.
        /// </summary>
        public InvalidLineNumberException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLineNumberException" /> class.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public InvalidLineNumberException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLineNumberException" /> class.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        /// <param name="inner">The inner exception.</param>
        public InvalidLineNumberException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
