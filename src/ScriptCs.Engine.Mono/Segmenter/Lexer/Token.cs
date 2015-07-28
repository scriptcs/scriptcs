namespace ScriptCs.Engine.Mono.Segmenter.Lexer
{
    public static class Token
    {
        public const int Eof = -1;
        public const int Identifier = -2;
        public const int String = -3;
        public const int Character = -4;
        public const int Do = -5;
        public const int While = -6;
        public const int If = -7;
        public const int Else = -8;
        public const int LeftBracket = '{';
        public const int RightBracket = '}';
        public const int LeftParenthese = '(';
        public const int RightParenthese = ')';
        public const int SemiColon = ';';
        public const int ForwardSlash = '/';
        public const int EscapeChar = '\\';
        public const int Star = '*';
        public const int Space = ' ';
        public const int Tab = '\t';
        public const int LineFeed = '\r';
        public const int NewLine = '\n';
        public const int Quote = '"';
        public const int SingleQuote = '\'';
    }
}