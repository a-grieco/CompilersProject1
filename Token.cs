namespace project1
{
    // $Id: Token.cs  2017-03-24 leblanc $
    // Translated from Token.java 2010-01-07 17:14:40Z cytron $
    public class Token
	{

		//
		// Java enums would be better for this,
		//   but these statics allow a closer match
		//   of the code here to the code in the text book
		//
		public const int EOF = 0, FLTDCL = 1, INTDCL = 2, PRINT = 3, 
            ASSIGN = 4, PLUS = 5, MINUS = 6, ID = 7, INUM = 8, FNUM = 9,
            STABLE = 10;

		public static readonly string[] token2str = new string[] {"$",
            "fltdcl", "intdcl", "print", "assign", "plus", "minus", "id",
            "inum", "fnum", "stable"};

		public readonly int type;
		public readonly string val;

		public Token(int type) : this(type, "")
		{
		}

		public Token(int type, string val)
		{
			this.type = type;
			this.val = val;
		}

		public override string ToString()
		{
			return "Token type\t" + token2str[type] + "\tval\t" + val;
		}
	}

}