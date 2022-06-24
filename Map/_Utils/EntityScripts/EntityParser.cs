using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


namespace NPKEVIN.Utils
{
    // LL(1) Parser for Entity
    class EntityParser
    {
        StreamReader inputStream;
        Stack<Symbol> ss;
        Dictionary<Symbol, Dictionary<Symbol, Rule>> t;

        List<VALVE.Entity> Entities;

        bool isKey;
        string lastKey;
        int index;

        // Recieve all entities (char) byte[]
        public EntityParser(byte[] data)
        {
            inputStream = new StreamReader(new MemoryStream(data));
        }

        ~EntityParser()
        {
            inputStream.Close();
        }

        private Symbol lexer(int c)
        {
            if (Char.IsWhiteSpace((char)c)) return Symbol.whitespace;
            if (Char.IsLetter((char)c)) return Symbol.validStr;
            if (Char.IsNumber((char)c)) return Symbol.validStr;

            switch (c)
            {
                case '*': return Symbol.validStr;
                case '!': return Symbol.validStr;
                case '_': return Symbol.validStr;
                case '-': return Symbol.validStr;
                case '.': return Symbol.validStr;
                case ',': return Symbol.validStr;
                case '{': return Symbol.lcurly;
                case '}': return Symbol.rcurly;
                case '"': return Symbol.dbquote;
                case '/': return Symbol.fslash;
                case '\\': return Symbol.bslash;

                case -1: return Symbol.EOS;
                case 0: return Symbol.EOS;
                default: return Symbol.INVALID;
            }
        }

        private void SetRules()
        {
            // Fill Table[i] to avoid KeyNotFound Exception (null)
            foreach (Symbol i in Enum.GetValues(typeof(Symbol)))
                t[i] = new Dictionary<Symbol, Rule>();


            // [ExpectingSymbol][RecievedSymbol] = Rule.toApply;
            t[Symbol.ENTs][Symbol.lcurly] = Rule.ExpandENTs;

            t[Symbol.ENT][Symbol.lcurly] = Rule.EnterBlock;

            t[Symbol.KVPs][Symbol.dbquote] = Rule.ExpandKVPs;
            t[Symbol.KVPs][Symbol.rcurly] = Rule.ExitBlock;

            t[Symbol.KVP][Symbol.dbquote] = Rule.ExpandKVP;

            t[Symbol.KEY][Symbol.validStr] = Rule.ReadSTR;
            t[Symbol.VALUE][Symbol.validStr] = Rule.ReadSTR;

            t[Symbol.ENTs][Symbol.EOS] = Rule.SkipSymbol;
        }

        public List<VALVE.Entity> Parse()
        {
            ResetState();
            SetRules();

            // Init Symbol Stack
            ss.Push(Symbol.EOS);
            ss.Push(Symbol.ENTs);

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
                    else if (lexer(inputStream.Peek()) == ss.Peek())
                    {
                        //Debug.Log("Consuming: [ " + (char)inputStream.Peek() + " : " + inputStream.Peek() + " ]");
                        inputStream.Read();
                        ss.Pop();
                    }
                    else
                    {
                        //Debug.Log(ss.Peek() + " :: " + lexer(inputStream.Peek()) );
                        //Debug.Log("Rule: " + t[ss.Peek()][lexer(inputStream.Peek())] + "\n" +
                        //    (char)inputStream.Peek() + " >> [ " + ss.Peek() + " : " + lexer(inputStream.Peek()) + " ]");
                        switch (t[ss.Peek()][lexer(inputStream.Peek())])
                        {
                            case Rule.ExpandENTs:
                                ss.Pop();
                                ss.Push(Symbol.ENTs);
                                ss.Push(Symbol.ENT);
                                break;
                            case Rule.ExpandKVP:
                                ss.Pop();
                                ss.Push(Symbol.dbquote);
                                ss.Push(Symbol.VALUE);
                                ss.Push(Symbol.dbquote);

                                ss.Push(Symbol.dbquote);
                                ss.Push(Symbol.KEY);
                                ss.Push(Symbol.dbquote);
                                break;
                            case Rule.ExpandKVPs:
                                ss.Pop();
                                ss.Push(Symbol.KVPs);
                                ss.Push(Symbol.KVP);
                                break;

                            case Rule.ReadSTR:
                                isKey = ss.Peek() == Symbol.KEY;
                                ss.Pop();
                                string str = ReadString();
                                if (!isKey)
                                {
                                    if (Entities.Count == index)
                                        Entities.Add(new VALVE.Entity());

                                    Entities[index].AddParam(lastKey, str);
                                    lastKey = null;
                                }
                                else
                                {
                                    lastKey = str;
                                }
                                break;
                            case Rule.EnterBlock:
                                ss.Pop();
                                ss.Push(Symbol.rcurly);
                                ss.Push(Symbol.KVPs);
                                ss.Push(Symbol.lcurly);
                                break;
                            case Rule.ExitBlock:
                                ss.Pop();
                                index++;
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
                Debug.LogError(e);
                Debug.LogError(ss.Peek() + " :: " + lexer(inputStream.Peek()) + "   INDEX: " + index);
                Debug.LogError(inputStream.Peek() + "  :  " + (char)inputStream.Peek());
                Debug.Log(inputStream.ReadLine());
                return null;
            }
            return Entities;
        }

        // Reads string until ", ignore escapes
        private string ReadString()
        {
            string tmp_str = "";
            while (inputStream.Peek() != '\n')
            {
                // Process escape characters
                if (inputStream.Peek() == '\\')
                {
                    inputStream.Read(); // discard escape char '\'
                    tmp_str += (char)inputStream.Read();
                }
                else if (inputStream.Peek() == '"')
                    break;
                else
                    tmp_str += (char)inputStream.Read();
            }
            return tmp_str.ToLower();
        }

        private void ResetState()
        {
            isKey = false;
            lastKey = null;
            index = 0;
            ss = new Stack<Symbol>();
            t = new Dictionary<Symbol, Dictionary<Symbol, Rule>>();
            Entities = new List<VALVE.Entity>();
            inputStream.BaseStream.Seek(0, SeekOrigin.Begin);
            inputStream.DiscardBufferedData();
        }

        enum Symbol
        {
            // Terminals
            INVALID, // Default value (0)
            EOS,
            whitespace,
            validStr,
            lcurly,
            rcurly,
            dbquote,
            fslash,
            bslash,

            // Non-Terminals
            ENT,
            ENTs,
            KVP,
            KVPs,
            KEY,
            VALUE,
            STR,
        }

        // At least 1 rule for each Non-Terminal
        enum Rule
        {
            UNDEFINED, // Default value (0)
            SkipSymbol,
            SkipInputChar,

            ExpandENT,
            ExpandENTs,
            ExpandKVP,
            ExpandKVPs,

            EnterBlock,
            ExitBlock,

            ReadSTR,
            ReadNUMs,
        }
    }
}
