using System.Collections.Generic;
using System.Text.RegularExpressions;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class LineAnalyzer : ILineAnalyzer
    {
        public string CurrentText { get; private set; }
        public LineState CurrentState { get; private set; }
        public int TextPosition { get; private set; }
        
        private readonly IDictionary<Regex, LineState> patternMap = new Dictionary<Regex, LineState>();

        public LineAnalyzer()
        {
            CurrentText = null;
            CurrentState = LineState.Unknown;
            TextPosition = 0;

            PopulatePatternList();
        }

        public void Analyze(string line)
        {
            foreach (var p in patternMap)
            {
                var m = p.Key.Match(line);

                if (m.Success)
                {
                    var g = m.Groups[1];
                    CurrentText = g.Value;
                    TextPosition = g.Index;
                    CurrentState = p.Value;

                    return;
                }
            }

            CurrentText = null;
            TextPosition = -1;
            CurrentState = LineState.Unknown;
        }

        private void PopulatePatternList()
        {
            patternMap[new Regex(@"^\s*#r\s+(.*)$")] = LineState.AssemblyName;
            patternMap[new Regex(@"^\s*#load\s+(.*)$")] = LineState.FilePath;
        }
    }
}
