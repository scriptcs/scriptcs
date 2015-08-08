using System;
using System.Collections.Generic;

using ScriptCs.Engine.Mono.Segmenter.Lexer;

namespace ScriptCs.Engine.Mono.Segmenter.Parser
{
    public sealed class RegionParser : IDisposable
    {
        private ScriptLexer _lexer;
        private LexerResult _current;

        public List<RegionResult> Parse(string code)
        {
            _lexer = new ScriptLexer(code);
            _current = _lexer.GetToken();
            return GetRegionBlocks();
        }

        public void Dispose()
        {
            _lexer.Dispose();
        }

        private List<RegionResult> GetRegionBlocks()
        {
            var result = new List<RegionResult>();
            while (true)
            {
                RegionResult region;
                switch (_current.Token)
                {
                    case Token.Eof: return result;
                    case Token.Do: // do-while has two blocks
                        region = ParseBlock();
                        _current = _lexer.GetToken();
                        if (_current.Token == Token.While)
                        {
                            var block = ParseBlock();
                            region = region.Combine(block);
                            _current = _lexer.GetToken();
                        }
                        result.Add(region);
                        break;
                    case Token.If:
                        region = ParseBlock();
                        _current = _lexer.GetToken();
                        while(_current.Token == Token.Else)
                        {
                            var block = ParseBlock();
                            region = region.Combine(block);
                            _current = _lexer.GetToken();
                        }
                        result.Add(region);
                        break;
                    default:
                        region = ParseBlock();
                        result.Add(region);
                        _current = _lexer.GetToken();
                        break;
                }
            }
        }

        private RegionResult ParseBlock()
        {
            var start = _current.Start;

            // first token is Left curly bracket.
            bool block = _current.Token == Token.LeftBracket;

            while (_current.Token != Token.Eof)
            {
                _current = _lexer.GetToken();

                if ((!block && _current.Token == Token.SemiColon)
                    || (block && _current.Token == Token.RightBracket)
                    || _current.Token == Token.Eof)
                {
                    return new RegionResult
                    {
                        Offset = start,
                        Length = _current.End - start
                    };
                }

                if (_current.Token == Token.LeftParenthese)
                {
                    var isComplete = SkipScope(Token.LeftParenthese, Token.RightParenthese);
                    if (_current.Token == Token.Eof)
                    {
                        return new RegionResult
                        {
                            Offset = start,
                            Length = _current.End - start,
                            IsCompleteBlock = isComplete
                        };
                    }

                    continue;
                }

                if (_current.Token == Token.LeftBracket)
                {
                    bool isComplete = SkipScope(Token.LeftBracket, Token.RightBracket);
                    return new RegionResult
                    {
                        Offset = start,
                        Length = _current.End - start,
                        IsCompleteBlock = isComplete
                    };
                }
            }

            throw new InvalidOperationException(string.Format("{0} should never reach this point.", typeof(RegionParser).Name));
        }

        private bool SkipScope(int leftToken, int rightToken)
        {
            if (_current.Token != leftToken)
            {
                throw new ArgumentException("Invalid use of SkipBlock method, current token should equal left token parameter");
            }

            var scope = new Stack<int>();
            scope.Push(1);

            while (_current.Token != Token.Eof)
            {
                _current = _lexer.GetToken();

                if (_current.Token == leftToken)
                {
                    scope.Push(1);
                }

                if (_current.Token == rightToken)
                {
                    scope.Pop();
                }

                if (scope.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}