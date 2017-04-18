using System.IO;

namespace project1
{
    // $Id: TokenStream.cs  2017-03-24 leblanc $
    // Translated from TokenStream.java 22 2010-01-07 16:50:05Z cytron $

    public class TokenStream
    {
        private Scanner scanner;
		private Token nextToken;

        public TokenStream(FileStream file)
        {
            scanner = new Scanner(file);
            advance();
        }

		public virtual int peek()
		{
			return nextToken.type;
		}

        public virtual string peekValue()
        {
            return nextToken.val;
        }

		public virtual Token advance()
		{
			Token ans = nextToken;
		    int type = scanner.yylex();
		    string val= scanner.yytext;
		    nextToken = new Token(type, val);
			return ans;
		}

	}

}