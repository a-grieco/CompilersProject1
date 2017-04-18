using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;
using Microsoft.SqlServer.Server;

namespace project1
{
    using static Token;

    // $Id: Parser.java 25 2010-01-07 17:03:40Z cytron $

    /// <summary>
    /// Recursive-descent parser based on the grammar given
    ///   in Figure 2.1
    /// @author cytron
    /// 
    /// </summary>
    public class Parser
    {
        public struct SymbolTableValue
        {
            private bool _isSet;
            private bool _isLambda;
            private int _type;
            private float _value;

            public bool IsSet
            {
                get { return _isSet; }
            }

            public bool IsLambda
            {
                get { return _isLambda; }
                set { _isLambda = value; }
            }

            public int Type
            {
                get { return _type; }
                set
                {
                    if (value == Token.INUM || value == Token.FNUM)
                    {
                        _type = value;
                    }
                    else
                    {
                        Console.WriteLine("Expecting INUM or FNUM");
                    }
                }
            }

            public float Value
            {
                get { return _value; }
                set
                {
                    _isSet = true;
                    _value = value;
                }
            }

            public override string ToString()
            {
                string str = "isSet: " + _isSet + ", ";
                if (_type == Token.INUM)
                {
                    str += "INUM ";
                    if (_isSet)
                    {
                        str += (int)_value;
                    }
                    else
                    {
                        str += "[]";
                    }
                }
                else if (_type == Token.FNUM)
                {
                    str += "FNUM ";
                    if (_isSet)
                    {
                        str += _value;
                    }
                    else
                    {
                        str += "[]";
                    }
                }
                else
                {
                    str += "<no type> ";
                }
                return str;
            }
        }

        Dictionary<char, SymbolTableValue> symbolTable =
            new Dictionary<char, SymbolTableValue>();

        private void AddToSymbolTable(int type, string id)
        {
            // prevent duplicate declaration of the same variable
            if (symbolTable.ContainsKey(Convert.ToChar(id)))
            {
                int idType = symbolTable[Convert.ToChar(id)].Type;
                string typeName = idType == Token.INUM ? "INUM" :
                    (idType == Token.FNUM ? "FNUM" : "UNDEFINED");
                Warning("Variable " + id + " already declared as " + typeName);
            }
            else
            {
                SymbolTableValue stValue = new SymbolTableValue();
                stValue.Type = type;
                symbolTable.Add(Convert.ToChar(id), stValue);
            }
        }

        // Assigns a value to the symbol in the table; prevents assignment of a
        // float value to a symbol declared as an int
        private void UpdateSymbolTable(char id, SymbolTableValue stValue)
        {
            if (symbolTable[id].Type == Token.INUM)
            {
                if (stValue.Type == Token.FNUM)
                {
                    Warning("Cannot convert from FNUM to INUM");
                }
                else
                {
                    symbolTable[id] = stValue;
                }
            }
            else if (symbolTable[id].Type == Token.FNUM)
            {
                stValue.Type = Token.FNUM;
                symbolTable[id] = stValue;
            }
            else
            {
                Error("Symbol type (FNUM/INUM) not found.");
            }
        }

        // TODO: what exactly should this print?
        private void Print(string id)
        {
            if (symbolTable.ContainsKey(Convert.ToChar(id)))
            {
                Console.WriteLine(id + " = " +
                                  symbolTable[Convert.ToChar(id)].Value);
            }
            else
            {
                Console.WriteLine(id + " is unassigned");
            }
        }

        // Prints the symbol table ("p ~")
        private void PrintSymbolTable()
        {
            Console.WriteLine("*---------------------------------------*");
            foreach (KeyValuePair<char, SymbolTableValue> st in symbolTable)
            {
                Console.WriteLine("Key = {0}, Value = {1}", st.Key, st.Value);
            }
            Console.WriteLine("*---------------------------------------*");
        }

        private TokenStream ts;

        public Parser(FileStream file)
        {
            ts = new TokenStream(file);
        }

        public virtual void Prog()
        {
            if (ts.peek() == FLTDCL || ts.peek() == INTDCL || ts.peek() == ID ||
                ts.peek() == PRINT || ts.peek() == EOF)
            {
                Things();
                Expect(EOF);
            }
            else
            {
                Error("expected floatdcl, intdcl, id, print, or eof");
            }
        }

        public virtual void Things()
        {
            if (ts.peek() == FLTDCL || ts.peek() == INTDCL || ts.peek() == ID
                || ts.peek() == PRINT)
            {
                Thing();
                Things();
            }
            else if (ts.peek() == EOF)
            {
                // do nothing for lambda-production
            }
            else
            {
                Error("expected floatdcl, intdcl, id, or print");
            }
        }

        private void Thing()
        {
            if (ts.peek() == FLTDCL || ts.peek() == INTDCL)
            {
                Dcl();
            }
            else if (ts.peek() == ID || ts.peek() == PRINT)
            {
                Stmt();
            }
        }

        public virtual void Dcl()
        {
            string id;

            if (ts.peek() == FLTDCL)
            {
                Expect(FLTDCL);
                id = ts.peekValue();
                Expect(ID);
                AddToSymbolTable(Token.FNUM, id);
            }
            else if (ts.peek() == INTDCL)
            {
                Expect(INTDCL);
                id = ts.peekValue();
                Expect(ID);
                AddToSymbolTable(Token.INUM, id);
            }
            else
            {
                Error("expected float or int declaration");
            }
        }

        public virtual void Stmt()
        {
            string id;
            int op;
            SymbolTableValue idVal;

            if (ts.peek() == ID)
            {
                id = ts.peekValue();
                if (!symbolTable.ContainsKey(Convert.ToChar(id)))
                {
                    Warning(id + " was not declared.");
                    //absorb remaining stmt
                    ts.advance();
                    ts.advance();
                    SymbolTableValue stDummyVal = Val();
                    Expr(stDummyVal);
                }
                else
                {
                    idVal = symbolTable[Convert.ToChar(id)];

                    Expect(ID);
                    op = ts.peek();
                    Expect(ASSIGN);
                    SymbolTableValue stVal = Val();
                    if (!stVal.IsSet)
                    {
                        Console.WriteLine("Assignment of " + id + " failed.");
                        // assignment failed, absorb remaining stmt
                        stVal.Type = Token.FNUM;
                        Expr(stVal);
                    }
                    else
                    {
                        // if value is valid, evaluate expression
                        SymbolTableValue stExp = Expr(idVal);
                        // if expression is valid (and not lambda), perform 
                        // operation and update symbol table
                        if (stExp.IsSet)
                        {
                            SymbolTableValue stResult = Operate(
                                symbolTable[Convert.ToChar(id)],
                                Token.PLUS, stVal, stExp);
                            UpdateSymbolTable(Convert.ToChar(id), stResult);
                        }
                        // if expression is lambda, update symbol table with value
                        else if (stExp.IsLambda)
                        {
                            UpdateSymbolTable(Convert.ToChar(id), stVal);
                        }
                    }

                }
            }
            else if (ts.peek() == PRINT)
            {
                Expect(PRINT);

                // add print symbol table functionality
                // TODO: grammar could be adjusted to accomodate this
                if (ts.peek() == STABLE)
                {
                    Expect(STABLE);
                    PrintSymbolTable();
                }
                else
                {
                    id = ts.peekValue();
                    Expect(ID);
                    Print(id);
                }
            }
            else
            {
                Error("expected id or print");
            }
        }


        // better to pass the value into expression and do everything that way
        public virtual SymbolTableValue Expr(SymbolTableValue stValue)
        {
            int op = ts.peek();
            if (ts.peek() == PLUS)
            {
                Expect(PLUS);
                SymbolTableValue v = Val();
                SymbolTableValue e = Expr(stValue);
                return Operate(stValue, op, v, e);
            }
            else if (ts.peek() == MINUS)
            {
                Expect(MINUS);
                SymbolTableValue v = Val();
                SymbolTableValue e = Expr(stValue);
                return Operate(stValue, op, v, e);
            }
            else if (ts.peek() == FLTDCL || ts.peek() == INTDCL ||
                ts.peek() == ID || ts.peek() == PRINT || ts.peek() == EOF)
            {
                // Do nothing for lambda-production

                // return (unset) flagged lambda value for lambda-production
                SymbolTableValue stLambda = new SymbolTableValue();
                stLambda.IsLambda = true;
                return stLambda;
                //stLambda.Type = stValue.Type;
                //stLambda.Value = 0;
                //return stLambda;
            }
            else
            {
                Error("expected plus, minus, fltdcl, intdcl, id, print, or " +
                      "eof");
            }
            Console.WriteLine("I SHOULD NOT BE HERE...TODO DELETE ME");
            return new SymbolTableValue(); // code should not reach this point 
        }

        // Adds or subtracts two values and returns a SymbolTableValue with an
        // appropriate type: INUM + INUM = INUM, FNUM + (INUM or FNUM) = FNUM
        // TODO: check for valid assignment to variable (different method)
        private SymbolTableValue Operate(SymbolTableValue idValue, int op,
            SymbolTableValue v, SymbolTableValue e)
        {
            SymbolTableValue result = new SymbolTableValue();

            // check for valid type 
            if (idValue.Type == INUM)
            {
                // if destination var is INUM, both operands must be INUM
                if (v.Type == INUM && (e.IsLambda || e.Type == INUM))
                {
                    result.Type = INUM;
                }
                else
                {
                    //Error("Cannot convert from FNUM to INUM type");
                    Warning("Cannot convert from FNUM to INUM type");
                }
            }
            else if (idValue.Type == FNUM)
            {
                result.Type = FNUM;
            }
            else
            {
                Error("Unrecognized type (expected INUM or FNUM)");
            }

            // if type is valid, complete expression
            if (result.Type == Token.INUM || result.Type == Token.FNUM)
            {
                // perform operation
                if (op == Token.PLUS)
                {
                    if (e.IsLambda)
                    {
                        Console.WriteLine("PLUS " + v.Value + " (Expr() = lambda) ");
                        result.Value = v.Value + e.Value;
                    }
                    else
                    {
                        Console.WriteLine("PLUS " + v.Value + " + " + e.Value);
                        result.Value = v.Value + e.Value;
                    }
                }
                else if (op == Token.MINUS)
                {
                    if (e.IsLambda)
                    {
                        Console.WriteLine("MINUS (-)" + v.Value + " (Expr() = lambda) ");
                        result.Value = - v.Value;
                    }
                    else
                    {
                        Console.WriteLine("MINUS (-)" + v.Value + " + " + e.Value);
                        result.Value = -v.Value + e.Value;
                    }
                }
            } // otherwise return value has isSet = false, with no value/type

            return result;
        }

        public virtual SymbolTableValue Val()
        {
            string value;

            if (ts.peek() == ID)
            {
                value = ts.peekValue();
                // ID's type/#value should already exist in symbol table 
                if (!symbolTable.ContainsKey(Convert.ToChar(value)))
                {
                    Warning(value + " is undefined");
                    ts.advance();
                    return new SymbolTableValue();
                }
                else
                {
                    Expect(ID);
                    return symbolTable[Convert.ToChar(value)];
                }
            }
            else if (ts.peek() == INUM)
            {
                value = ts.peekValue();
                Expect(INUM);
                SymbolTableValue stVal = new SymbolTableValue();
                stVal.Type = INUM;
                stVal.Value = Single.Parse(value);
                return stVal;
            }
            else if (ts.peek() == FNUM)
            {
                value = ts.peekValue();
                Expect(FNUM);
                SymbolTableValue stVal = new SymbolTableValue();
                stVal.Type = FNUM;
                stVal.Value = Single.Parse(value);
                return stVal;
            }
            else
            {
                Error("expected id, inum, or fnum");
                return new SymbolTableValue();
            }
        }

        private void Expect(int type)
        {
            Token t = ts.advance();
            if (t.type != type)
            {
                throw new Exception("Expected type " + token2str[type] +
                    " but received type " + token2str[t.type]);
            }
        }

        private void Error(string message)
        {
            throw new Exception(message);
        }

        private void Warning(string message)
        {
            Console.WriteLine("Warning: " + message);
        }
    }

}