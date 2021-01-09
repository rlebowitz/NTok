using System;

/*
 * NTok
 * A configurable tokenizer implemented in C# based on the Java JTok tokenizer.
 *
 * (c) 2003 - 2014  DFKI Language Technology Lab http://www.dfki.de/lt
 *   Author: Joerg Steffen, steffen@dfki.de
 *
 * (c) 2021 - Finaltouch IT LLC
 *   Author:  Robert Lebowitz, lebowitz@finaltouch.com
 *
 *   This program is free software; you can redistribute it and/or
 *   modify it under the terms of the GNU Lesser General Public
 *   License as published by the Free Software Foundation; either
 *   version 2.1 of the License, or (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *   Lesser General Public License for more details.
 *
 *   You should have received a copy of the GNU Lesser General Public
 *   License along with this library; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

namespace NetTok.Tokenizer.Exceptions
{
    /// <summary>
    ///     <seealso cref="InitializationException" /> is thrown when the tokenizer can't be initialized.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class InitializationException : Exception
    {
        /// <summary>
        ///     Creates a new instance of <seealso cref="InitializationException" /> with null as its detail message. The
        ///     cause is not initialized.
        /// </summary>
        public InitializationException() { }


        /// <summary>
        ///     Creates a new instance of <seealso cref="InitializationException" /> with the given detail message. The
        ///     cause is not initialized.
        /// </summary>
        /// <param name="message">
        ///     the detail message
        /// </param>
        public InitializationException(string message) : base(message) { }


        /// <summary>
        ///     Creates a new instance of <seealso cref="InitializationException" /> with the specified cause and a detail
        ///     message of (cause==null ? null : cause.toString()) (which typically contains the class and
        ///     detail message of cause).
        /// </summary>
        /// <param name="cause">A throwable with the cause of the exception.</param>
        public InitializationException(Exception cause) : base(cause.Message, cause) { }


        /// <summary>
        ///     Creates a new instance of <seealso cref="InitializationException" /> with the given detail message and the
        ///     given cause.
        /// </summary>
        /// <param name="message">The detail message.</param>
        /// <param name="cause">A throwable with the cause of the exception.</param>
        public InitializationException(string message, Exception cause) : base(message, cause) { }
    }
}