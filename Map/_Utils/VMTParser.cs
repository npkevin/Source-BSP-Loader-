using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


/* TODO:
 *   - Implement [VAL, {1,2,3}] // conflict with value as block
 *   - Implement [VAL, digit] handling... currently reading as str
 *   - dbquoted VAL are not treated as digit, fix that
 */

namespace NPKEVIN.Utils
{
    // LL(1) Parser for VMT
    class VMTParser
    {
        public char Seperator = '/';

        string Search = null;
        object Found = null;
        Stack<string> dirStack = new Stack<string>();
        Stack<string> ridStack = new Stack<string>();

        // Dictionary of Rules
        StreamReader inputStream;
        Stack<Symbol> ss = new Stack<Symbol>();
        Dictionary<Symbol, Dictionary<Symbol, Rule>> t = new Dictionary<Symbol, Dictionary<Symbol, Rule>>();

        bool isKey = false;
        string lastKey = null;

        // Assigned when parsed atleast once
        public string shader = null;
        public string PATH = null;

        public VMTParser(byte[] data)
        {
            inputStream = new StreamReader(new MemoryStream(data));
            SetRules();
        }

        public VMTParser(string path)
        {
            PATH = path;
            inputStream = new StreamReader(File.OpenRead(PATH));
            SetRules();
        }

        ~VMTParser()
        {
            inputStream.Close();
        }

        private Symbol lexer(int c)
        {
            if (Char.IsWhiteSpace((char)c)) return Symbol.whitespace;
            if (Char.IsLetter((char)c)) return Symbol.validChar;
            if (Char.IsNumber((char)c)) return Symbol.digit;

            switch (c)
            {
                // words (strings with no dbQuote)
                case '-': return Symbol.validChar;
                case '_': return Symbol.validChar;
                case '$': return Symbol.validChar;
                case '%': return Symbol.validChar;

                case '/': return Symbol.fslash;
                case '\\': return Symbol.bslash;
                case '{': return Symbol.lcurly;
                case '}': return Symbol.rcurly;
                case '[': return Symbol.lsquare;
                case ']': return Symbol.rsquare;
                case '"': return Symbol.dbquote;
                case '.': return Symbol.period;
                case -1: return Symbol.EOS;
                default: return Symbol.INVALID;
            }
        }

        private void SetRules()
        {
            // Fill Table[i] to avoid KeyNotFound Exception (null)
            foreach (Symbol i in Enum.GetValues(typeof(Symbol)))
                t[i] = new Dictionary<Symbol, Rule>();

            // [ExpectingSymbol][RecievedSymbol] = Rule.toApply;
            t[Symbol.KVPs][Symbol.EOS] = Rule.SkipSymbol;
            // KVP
            t[Symbol.KVP][Symbol.dbquote] = Rule.ExpandKVP;
            t[Symbol.KVP][Symbol.validChar] = Rule.ExpandKVP;
            t[Symbol.KVP][Symbol.digit] = Rule.ExpandKVP;
            // KVPS
            t[Symbol.KVPs][Symbol.dbquote] = Rule.ExpandKVPS;
            t[Symbol.KVPs][Symbol.validChar] = Rule.ExpandKVPS;
            t[Symbol.KVPs][Symbol.digit] = Rule.ExpandKVPS;

            // KEY/VALUE
            t[Symbol.KEY][Symbol.dbquote] = Rule.ReadSTR;
            t[Symbol.VAL][Symbol.dbquote] = Rule.ReadSTR;
            t[Symbol.KEY][Symbol.validChar] = Rule.ReadWORD;
            t[Symbol.VAL][Symbol.validChar] = Rule.ReadWORD;
            t[Symbol.VAL][Symbol.digit] = Rule.ReadNUM;
            t[Symbol.VAL][Symbol.period] = Rule.ReadNUM;
            t[Symbol.VAL][Symbol.lsquare] = Rule.ReadNUMArr;

            // Path
            t[Symbol.VAL][Symbol.lcurly] = Rule.EnterBlock; // TODO: CONFLICT w/ curlybraced-enclosed number arrays
            t[Symbol.KVPs][Symbol.rcurly] = Rule.ExitBlock;
        }

        private void SearchVMT() //Parse
        {
            string dbg = "";

            // Init Symbol Stack
            ss.Push(Symbol.EOS);
            ss.Push(Symbol.KVP);

            try
            {
                while (ss.Count > 0)
                {
                    // Skip all whitespaces in between symbols
                    if (lexer(inputStream.Peek()) == Symbol.whitespace)
                    {
                        inputStream.Read();
                    }
                    // Skip comments
                    // TODO: Better comment detection
                    else if (lexer(inputStream.Peek()) == Symbol.fslash || lexer(inputStream.Peek()) == Symbol.bslash)
                    {
                        inputStream.ReadLine();
                    }
                    // Consume matching CHARACTERS [same][same]
                    else if (lexer(inputStream.Peek()) == ss.Peek())
                    {
                        //Debug.Log("Consuming: [ " + (char)inputStream.Peek() + " : " + inputStream.Peek() + " ]");
                        inputStream.Read();
                        ss.Pop();
                    }
                    else
                    {
                        //Debug.Log(ss.Peek() + " :: " + lexer(inputStream.Peek()));
                        //Debug.Log("Rule: " + t[ss.Peek()][lexer(inputStream.Peek())] + "\n" +
                        //    (char)inputStream.Peek() + " >> [ " + ss.Peek() + " : " + lexer(inputStream.Peek()) + " ]");
                        switch (t[ss.Peek()][lexer(inputStream.Peek())])
                        {
                            case Rule.ExpandKVP:
                                ss.Pop();
                                ss.Push(Symbol.VAL);
                                ss.Push(Symbol.KEY);
                                break;
                            case Rule.ExpandKVPS:
                                ss.Pop();
                                ss.Push(Symbol.KVPs);
                                ss.Push(Symbol.KVP);
                                break;

                            // [KEY/VAL][validChar]
                            case Rule.ReadWORD:
                                isKey = ss.Peek() == Symbol.KEY;
                                ss.Pop();
                                string word = ReadWord();
                                if (!isKey)
                                {
                                    dbg += lastKey + " : " + word + '\n';
                                }
                                if (isKey)
                                {
                                    if (shader == null)
                                    {
                                        shader = word;
                                        dbg += shader + '\n';
                                    }
                                    lastKey = word;
                                }
                                // Only "finds" values that follow Get()'s path
                                else if (Search == lastKey && Search == dirStack.Peek())
                                {
                                    Found = word;
                                    lastKey = null;
                                }
                                break;
                            // [KEY/VAL][dbquote]
                            case Rule.ReadSTR:
                                isKey = ss.Peek() == Symbol.KEY;
                                ss.Pop();
                                string str = ReadString();
                                if (!isKey)
                                {
                                    dbg += lastKey + " : " + str + '\n';
                                }
                                if (isKey)
                                {
                                    if (shader == null)
                                    {
                                        shader = str;
                                        dbg += shader + '\n';
                                    }
                                    lastKey = str;
                                }
                                // Only "finds" values that follow Get()'s path
                                else if (Search == lastKey && Search == dirStack.Peek())
                                {
                                    Found = str;
                                    lastKey = null;
                                }
                                break;
                            // Always Values (ReadNUM & ReadNUMArr)
                            case Rule.ReadNUM:
                                ss.Pop();
                                float number = ReadNumber();
                                dbg += lastKey + " : " + number + '\n';
                                if (Search == lastKey && Search == dirStack.Peek())
                                {
                                    Found = number;
                                    lastKey = null;
                                }
                                break;
                            case Rule.ReadNUMArr:
                                ss.Pop();
                                float[] numArr = ReadNumberArray();
                                dbg += lastKey + " : " + numArr + '\n';
                                if (Search == lastKey && Search == dirStack.Peek())
                                {
                                    Found = numArr;
                                    lastKey = null;
                                }
                                break;



                            case Rule.EnterBlock:
                                if (dirStack.Peek() == lastKey || dirStack.Peek() == "*")
                                    ridStack.Push(dirStack.Pop());
                                ss.Pop();
                                ss.Push(Symbol.rcurly);
                                ss.Push(Symbol.KVPs);
                                ss.Push(Symbol.lcurly);
                                break;
                            case Rule.ExitBlock:
                                if (ridStack.Count > 0)
                                    dirStack.Push(ridStack.Pop());
                                ss.Pop();
                                break;



                            // Helper Rules
                            case Rule.SkipSymbol:
                                ss.Pop();
                                break;
                            case Rule.SkipInputChar:
                                inputStream.Read();
                                break;
                            default:
                                throw new Exception("Parsing Table Defaulted:  [ (char)" + inputStream.Peek() + " ]\n" +
                                    "[ " + ss.Peek() + " : " + lexer((char)inputStream.Peek()) + " ]    >> " + (char)inputStream.Peek()
                                );
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(dbg);
                Debug.LogError(e);
                Debug.LogError(ss.Peek() + " :: " + lexer(inputStream.Peek()));
                Debug.LogError((char)inputStream.Peek() + " >> " + inputStream.Peek() + ", LINE: " + inputStream.BaseStream.Position);
            }
        }

        // NOT CASE-SENSETIVE
        // Make sure to cast return to the value you are expecting. 
        public object SearchKey(string pathToKey)
        {
            ResetState();

            // path/to/key
            string[] dir = pathToKey.ToLower().Split(Seperator);
            Search = dir[dir.Length - 1];
            Found = null;

            dir = dir.Reverse().ToArray();
            foreach (string d in dir)
                dirStack.Push(d);
            SearchVMT();
            return Found;
        }

        // Reads string until whitespace
        private string ReadWord()
        {
            string tmp_str = "";
            while (lexer((char)inputStream.Peek()) != Symbol.whitespace)
                tmp_str += (char)inputStream.Read();
            return tmp_str.ToLower();
        }

        // Reads string until double-quote
        private string ReadString()
        {
            string tmp_str = "";

            //consume dbquote
            inputStream.Read();

            while (inputStream.Peek() != '"')
                tmp_str += (char)inputStream.Read();

            //consume dbquote
            inputStream.Read();

            return tmp_str.ToLower();
        }

        private float ReadNumber()
        {
            string tmp_str = "";
            while (inputStream.Peek() != '"' && (lexer((char)inputStream.Peek()) != Symbol.whitespace))
                tmp_str += (char)inputStream.Read();
            return float.Parse(tmp_str);
        }

        private float[] ReadNumberArray()
        {
            // Remove [ or {
            inputStream.Read();

            List<float> numArr = new List<float>();

            string tmp_str = "";
            while (inputStream.Peek() != ']' && inputStream.Peek() != '}')
                tmp_str += (char)inputStream.Read();

            // Remove ] or }
            inputStream.Read();

            Regex rgx = new Regex(@"\d*\.?\d+");
            MatchCollection matches = rgx.Matches(tmp_str);
            foreach (Match match in matches)
                numArr.Add(float.Parse(match.Value));


            return numArr.ToArray();
        }

        private void ResetState()
        {
            Search = null;
            Found = null;
            dirStack = new Stack<string>();
            ridStack = new Stack<string>();
            isKey = false;
            lastKey = null;
            ss = new Stack<Symbol>();
            inputStream.BaseStream.Seek(0, SeekOrigin.Begin);
            inputStream.DiscardBufferedData();
        }

        enum Symbol
        {
            // Terminals
            INVALID, // Default value (0)
            EOS,
            validChar,
            digit,
            period,
            dbquote,
            whitespace,
            fslash,
            bslash,
            lcurly,
            rcurly,
            lsquare,
            rsquare,

            // Non-Terminals
            KVP,
            KVPs,
            KEY,
            VAL,
            STR,
        }

        // At least 1 rule for each Non-Terminal
        enum Rule
        {
            UNDEFINED, // Default value (0)
            SkipSymbol,
            SkipInputChar,

            ExpandKVPS,
            ExpandKVP,

            EnterBlock,
            ExitBlock,

            ReadSTR,
            ReadWORD,
            ReadNUM,
            ReadNUMArr,
        }
    }
}
