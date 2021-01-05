using System;
using System.Collections.Generic;
using System.IO;

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

namespace NetTok.Tokenizer
{

	/// <summary>
	/// Provides static methods to work on files and stream.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public sealed class FileTools
	{

	  // would create a new instance of {@link FileTools}; not to be used
	  private FileTools()
	  {

		// private constructor to enforce noninstantiability
	  }


	  /// <summary>
	  /// Writes an input stream to a file. Fails if filename already exists.
	  /// </summary>
	  /// <param name="inputStream">
	  ///          some stream to be saved </param>
	  /// <param name="file">
	  ///          the target file </param>
	  /// <exception cref="IOException">
	  ///              when file can't be saved </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void saveStream(java.io.InputStream inputStream, java.io.File file) throws java.io.IOException
	  public static void saveStream(Stream inputStream, File file)
	  {

		int i;
		sbyte[] ab = new sbyte[4096];
		FileStream fos = null;
		try
		{
		  fos = new FileStream(file, FileMode.Create, FileAccess.Write);
		  while ((i = inputStream.Read(ab, 0, ab.Length)) > 0)
		  {
			fos.Write(ab, 0, i);
		  }
		}
		finally
		{
		  // always close
		  if (null != fos)
		  {
			fos.Close();
		  }
		}
	  }


	  /// <summary>
	  /// Reads a URL content to a string.
	  /// </summary>
	  /// <param name="url">
	  ///          some URL </param>
	  /// <returns> the content as a string or {@code null} if content could not be read </returns>
	  /// <exception cref="IOException">
	  ///              thrown when resource cannot be opened for reading </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String readUrlToString(java.net.URL url) throws java.io.IOException
	  public static string readUrlToString(URL url)
	  {

		URLConnection con = url.openConnection();
		con.connect();
		Stream @is = null;
		MemoryStream bos = null;

		// initialize size
		int len = con.ContentLength;
		if (-1 == len)
		{
		  len = 10000;
		}

		try
		{
		  bos = new MemoryStream(len);
		  @is = con.InputStream;
		  readInputStream(bos, @is);
		}
		finally
		{
		  // always close
		  if (null != @is)
		  {
			@is.Close();
		  }
		  if (null != bos)
		  {
			bos.Close();
		  }
		}
		if (null != bos)
		{
		  return bos.ToString();
		}
		return null;
	  }


	  /// <summary>
	  /// Reads a URL content to a byte array.
	  /// </summary>
	  /// <param name="url">
	  ///          some URL </param>
	  /// <returns> the content as a byte array or {@code null} if content could not be read </returns>
	  /// <exception cref="IOException">
	  ///              thrown when resource cannot be opened for reading </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] readUrlToByteArray(java.net.URL url) throws java.io.IOException
	  public static sbyte[] readUrlToByteArray(URL url)
	  {

		URLConnection con = url.openConnection();
		con.connect();
		Stream @is = null;
		MemoryStream bos = null;

		// initialize size
		int len = con.ContentLength;
		if (-1 == len)
		{
		  len = 10000;
		}

		try
		{
		  bos = new MemoryStream(len);
		  @is = con.InputStream;
		  readInputStream(bos, @is);
		}
		finally
		{
		  // always close
		  if (null != @is)
		  {
			@is.Close();
		  }
		  if (null != bos)
		  {
			bos.Close();
		  }
		}

		if (null != bos)
		{
		  return bos.toByteArray();
		}
		return null;
	  }


	  /// <summary>
	  /// Reads some input stream and writes it into an output stream. The method applies some efficient
	  /// buffering in byte arrays and is the basis for all read...-methods in this class.
	  /// </summary>
	  /// <param name="os">
	  ///          some output stream. </param>
	  /// <param name="is">
	  ///          some input stream. </param>
	  /// <exception cref="IOException">
	  ///              thrown when reading or writing fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void readInputStream(java.io.OutputStream os, java.io.InputStream is) throws java.io.IOException
	  public static void readInputStream(Stream os, Stream @is)
	  {

		sbyte[] buffer = new sbyte[4096];
		int readb;
		while (true)
		{
		  readb = @is.Read(buffer, 0, buffer.Length);
		  if (readb == -1)
		  {
			break;
		  }
		  os.Write(buffer, 0, readb);
		}
	  }


	  /// <summary>
	  /// Reads some input stream and return its content as a string.
	  /// </summary>
	  /// <param name="is">
	  ///          the input stream </param>
	  /// <param name="encoding">
	  ///          the encoding to use </param>
	  /// <returns> the content of the stream as string or {@code null} if content could not be read </returns>
	  /// <exception cref="IOException">
	  ///              if there is an error when reading the stream </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String readInputStream(java.io.InputStream is, String encoding) throws java.io.IOException
	  public static string readInputStream(Stream @is, string encoding)
	  {

		MemoryStream bos = null;
		try
		{
		  bos = new MemoryStream();
		  readInputStream(bos, @is);
		}
		finally
		{
		  // always close
		  if (null != bos)
		  {
			bos.Close();
		  }
		}

		if (null != bos)
		{
		  return bos.toString(encoding);
		}
		return null;
	  }


	  /// <summary>
	  /// Reads some input stream and return its content as byte array.
	  /// </summary>
	  /// <param name="is">
	  ///          the input stream </param>
	  /// <returns> the content of the stream as byte array or {@code null} if content could not be read </returns>
	  /// <exception cref="IOException">
	  ///              if there is an error when reading the stream </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] readInputStreamToByteArray(java.io.InputStream is) throws java.io.IOException
	  public static sbyte[] readInputStreamToByteArray(Stream @is)
	  {

		MemoryStream bos = null;
		try
		{
		  bos = new MemoryStream();
		  readInputStream(bos, @is);
		}
		finally
		{
		  // always close
		  if (null != bos)
		  {
			bos.Close();
		  }
		}

		if (null != bos)
		{
		  return bos.toByteArray();
		}
		return null;
	  }


	  /// <summary>
	  /// Recursively collects all filenames in the given directory with the given suffix and returns
	  /// them in a list.
	  /// </summary>
	  /// <param name="directory">
	  ///          the directory name </param>
	  /// <param name="suffix">
	  ///          the filename suffix </param>
	  /// <returns> a list with the filenames </returns>
	  public static IList<string> getFilesFromDir(string directory, string suffix)
	  {

		// initialize result list
		IList<string> fileNames = new List<string>();

		// add file separator to directory if necessary
		if (!directory.EndsWith(File.separator, StringComparison.Ordinal))
		{
		  directory = directory + File.separator;
		}

		// check if input is an directory
		File dirFile = new File(directory);
		if (!dirFile.Directory)
		{
		  return fileNames;
		}

		// iterate over files in directory
		File[] filesInDir = dirFile.listFiles();
		for (int i = 0; i < filesInDir.Length; i++)
		{
		  // if file is a directory, collect recursivly
		  if (filesInDir[i].Directory)
		  {
			((List<string>)fileNames).AddRange(getFilesFromDir(filesInDir[i].AbsolutePath, suffix));
		  }
		  else if (filesInDir[i].Name.EndsWith(suffix))
		  {
			// otherwise, add filename with matching suffix to result list
			fileNames.Add(filesInDir[i].AbsolutePath);
		  }
		}
		return fileNames;
	  }


	  /// <summary>
	  /// Copies a source file to a target file.
	  /// </summary>
	  /// <param name="source">
	  ///          the source file to copy </param>
	  /// <param name="target">
	  ///          the target file </param>
	  /// <exception cref="IOException">
	  ///           if copying fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void copyFile(java.io.File source, java.io.File target) throws java.io.IOException
	  public static void copyFile(File source, File target)
	  {

		FileStream fis = new FileStream(source, FileMode.Open, FileAccess.Read);
		FileStream fos = new FileStream(target, FileMode.Create, FileAccess.Write);
		sbyte[] buf = new sbyte[1024];
		int i = 0;
		while ((i = fis.Read(buf, 0, buf.Length)) != -1)
		{
		  fos.Write(buf, 0, i);
		}
		fis.Close();
		fos.Close();
	  }


	  /// <summary>
	  /// New NIO based method to read the contents of a file as byte array. Only files up to size
	  /// Integer.MAX_INT can be read. The byte buffer is rewinded when returned.
	  /// </summary>
	  /// <param name="file">
	  ///          the file to read </param>
	  /// <returns> the file content as byte array </returns>
	  /// <exception cref="IOException">
	  ///           if reading the content fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static ByteBuffer readFile(java.io.File file) throws java.io.IOException
	  public static ByteBuffer readFile(File file)
	  {

		FileStream fis = new FileStream(file, FileMode.Open, FileAccess.Read);
		FileChannel fc = fis.Channel;
		// for some reason, the buffer must be 1 byte bigger than the file
		ByteBuffer readBuffer = ByteBuffer.allocate((int)fc.size());
		fc.read(readBuffer);
		fis.Close();
		// also closes channel
		readBuffer.rewind();
		return readBuffer;
	  }


	  /// <summary>
	  /// New NIO based method to read a file as a String with the given encoding.
	  /// </summary>
	  /// <param name="file">
	  ///          the file to read </param>
	  /// <param name="encoding">
	  ///          the encoding to use for conversion, if {@code null} UTF-8 is used </param>
	  /// <returns> the file content as string </returns>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String readFileAsString(java.io.File file, String encoding) throws java.io.IOException
	  public static string readFileAsString(File file, string encoding)
	  {

		ByteBuffer buffer = readFile(file);
		if (null == encoding)
		{
		  encoding = "UTF-8";
		}
		string converted = new string(buffer.array(), encoding);
		return converted;
	  }


	  /// <summary>
	  /// Returns an input stream for the given resource.
	  /// </summary>
	  /// <param name="resourcePath">
	  ///          the resource path </param>
	  /// <returns> an input stream where to read from the content of the resource </returns>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the resource </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.InputStream openResourceFileAsStream(java.nio.file.Path resourcePath) throws java.io.IOException
	  public static Stream openResourceFileAsStream(Path resourcePath)
	  {

		// convert to OS agnostic representation for classpath lookup
		URI resourceUri = resourcePath.toUri();
		string cpLookupString = Paths.get("").toAbsolutePath().toUri().relativize(resourceUri).ToString();

		// first check for any user specific configuration in 'jtok-user'
		Stream @is = ClassLoader.getSystemResourceAsStream("jtok-user/" + cpLookupString);
		if (null == @is)
		{
		  @is = ClassLoader.getSystemResourceAsStream(cpLookupString);
		  if (null == @is)
		  {
			// try local loader with absolute path
			@is = typeof(FileTools).getResourceAsStream("/" + cpLookupString);
			if (null == @is)
			{
			  // try local loader, relative, just in case
			  @is = typeof(FileTools).getResourceAsStream(cpLookupString);
			  if (null == @is)
			  {
				// can't find it on classpath, so try relative to current directory
				// this will throw security exception under and applet but there's
				// no other choice left
				@is = Files.newInputStream(resourcePath);
			  }
			}
		  }
		}

		return @is;
	  }
	}

}