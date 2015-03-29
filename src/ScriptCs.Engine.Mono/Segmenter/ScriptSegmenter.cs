using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ScriptCs.Engine.Mono.Segmenter.Analyser;
using ScriptCs.Engine.Mono.Segmenter.Parser;

namespace ScriptCs.Engine.Mono.Segmenter
{
    public class ScriptSegmenter
    {
        public List<SegmentResult> Segment(string code)
        {
            const string ScriptPattern = @"#line 1.*?\n";
            var isScriptFile = Regex.IsMatch(code, ScriptPattern);
            if (isScriptFile)
            {
                // Remove debug line
                code = Regex.Replace(code, ScriptPattern, "");
            }

            var analyser = new CodeAnalyzer();
            var result = new List<SegmentResult>();
            using (var parser = new RegionParser())
            {
                foreach (var region in parser.Parse(code))
                {
                    // Calculate region linenumber
                    var lineNr = code.Substring(0, region.Offset).Count(x => x.Equals('\n'));

                    var segment = code.Substring(region.Offset, region.Length);

                    if (analyser.IsClass(segment))
                    {
                        result.Add(new SegmentResult
                        {
                            Type = SegmentType.Class,
                            BeginLine = lineNr,
                            Code = segment
                        });
                    }
                    else
                    {
                        var isMethod = analyser.IsMethod(segment);

                        if (isMethod)
                        {
                            // method ok
                            var method = analyser.ExtractPrototypeAndMethod(segment);

                            result.Add(new SegmentResult
                            {
                                Type = SegmentType.Prototype,
                                BeginLine = lineNr,
                                Code = method.ProtoType
                            });

                            result.Add(new SegmentResult
                            {
                                Type = SegmentType.Method,
                                BeginLine = lineNr,
                                Code = method.MethodExpression
                            });
                        }
                        else
                        {
                            result.Add(new SegmentResult
                            {
                                Type = SegmentType.Evaluation,
                                BeginLine = lineNr,
                                Code = segment
                            });
                        }
                    }
                }
            }

            return result
                .OrderBy(x => x.Type)
                .ToList();
        }
    }
}