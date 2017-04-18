using System;

namespace project1
{

    // $Id: CharStream.cs  2017-03-24 leblanc $
    // Translated from CharStream.java 2010-01-07 16:48:14Z cytron $

    /// <summary>
    /// Provides peek, EOF, and advance for the chapter 2 scanner
    /// @author cytron
    /// 
    /// </summary>
    public class CharStream
	{

		public const char BLANK = ' ';

		private readonly System.IO.StringReader @is;

		private char nextChar;
		private bool eof;

		public CharStream(System.IO.StringReader ds)
		{
			this.@is = ds;
			this.eof = false;
			this.nextChar = (char)0;
			advance();
		}

		public virtual char peek()
		{
			return nextChar;
		}

		public virtual bool EOF()
		{
			return eof;
		}

		public virtual char advance()
		{
			char ans = nextChar;
			try
			{
				int next = @is.Read();
				//
				//  If end of file, read will return -1
				//
				if (next == -1)
				{
					eof = true;
					nextChar = (char)0;
				}
				else
				{
					nextChar = (char) next;
				}
			}
			//
			//  On any problem, just assume end of input
			//
			catch (Exception t)
			{
				Console.WriteLine("Error encountered " + t);
				eof = true;
				return (char)0;
			}
			return ans;
		}
	}

}