%namespace project1
%scannertype Scanner
%visibility internal


%option stack, minimize, parser, verbose, persistbuffer, noembedbuffers, noparser

LineTerminator	(\r\n?|\n)
InputCharacter	[^\r\n]
WhiteSpace	({LineTerminator}|[ \t\f])
Slash	[/]
Minus	[\-]
NotSlashMinus	[^/\-]

/* The following code is emitted in the generated class
 *   You should use it when you find something interesting
 */
%{
	public void found(string s)
	{
	Console.WriteLine(String.Format("Found |{0}| from text ->> {1}", s, yytext));
	}
%}

%%

/* Finally, patterns of interest and what to
 *   upon finding them
 */


(" ")+			{ /* delete blanks */ }
"f"				{ return(Token.FLTDCL); }
"i"				{ return(Token.INTDCL); }
"p"				{ return(Token.PRINT); }
[a-eg-hj-oq-z]	{ return(Token.ID); }
[0-9]+"."[0-9]+	{ return(Token.FNUM); }
[0-9]+			{ return(Token.INUM); }
"="				{ return(Token.ASSIGN); }
"+"				{ return(Token.PLUS); }
"-"				{ return(Token.MINUS); }
"~"				{ return(Token.STABLE); }
.				{ /* ignore any unmatched characters */ }

%%