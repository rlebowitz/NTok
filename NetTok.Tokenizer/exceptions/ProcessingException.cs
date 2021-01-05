using System;

/*
 * JTok
 * A configurable tokenizer implemented in Java
 *
 * (C) 2003 - 2014  DFKI Language Technology Lab http://www.dfki.de/lt
 *   Author: Joerg Steffen, steffen@dfki.de
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

namespace NetTok.Tokenizer.exceptions
{
	/// <summary>
	/// <seealso cref="ProcessingException"/> is thrown when the processing of input data causes an error.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class ProcessingException : Exception
	{

	  /// <summary>
	  /// Creates a new instance of <seealso cref="ProcessingException"/> with null as its detail message. The
	  /// cause is not initialized.
	  /// </summary>
	  public ProcessingException() : base()
	  {

	  }


	  /// <summary>
	  /// Creates a new instance of <seealso cref="ProcessingException"/> with the given detail message. The cause
	  /// is not initialized.
	  /// </summary>
	  /// <param name="message">
	  ///          the detail message </param>
	  public ProcessingException(string message) : base(message)
	  {

	  }


	  /// <summary>
	  /// Creates a new instance of <seealso cref="ProcessingException"/> with the specified cause and a detail
	  /// message of (cause==null ? null : cause.toString()) (which typically contains the class and
	  /// detail message of cause).
	  /// </summary>
	  /// <param name="cause">
	  ///          a throwable with the cause of the exception (which is saved for later retrieval by the
	  ///          <seealso cref="getCause()"/> method). (A {@code null} value is permitted, and indicates that
	  ///          the cause is nonexistent or unknown.) </param>
	  public ProcessingException(Exception cause) : base(cause)
	  {

	  }


	  /// <summary>
	  /// Creates a new instance of <seealso cref="ProcessingException"/> with the given detail message and the
	  /// given cause.
	  /// </summary>
	  /// <param name="message">
	  ///          the detail message </param>
	  /// <param name="cause">
	  ///          a throwable with the cause of the exception (which is saved for later retrieval by the
	  ///          <seealso cref="getCause()"/> method). (A {@code null} value is permitted, and indicates that
	  ///          the cause is nonexistent or unknown.) </param>
	  public ProcessingException(string message, Exception cause) : base(message, cause)
	  {

	  }
	}

}