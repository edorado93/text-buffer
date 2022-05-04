// <copyright file="IInvalidBufferOperationException.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>


namespace TextBufferCommon.Exceptions
{
    public class InvalidBufferOperationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBufferOperationException" /> class.
        /// </summary>
        public InvalidBufferOperationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBufferOperationException" /> class.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public InvalidBufferOperationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBufferOperationException" /> class.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        /// <param name="inner">The inner exception.</param>
        public InvalidBufferOperationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
