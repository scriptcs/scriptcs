using System;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class PreProcessorUtilTests
    {
        public class GetPath : IDisposable
        {
            private string _envVar1Value;
            private string _envVar2Value;
            private string _envVar1Key;
            private string _envVar2Key;

            public GetPath()
            {
                _envVar1Key = "___ScriptCsKey1";
                _envVar2Key = "___ScriptCsKey2";
                _envVar1Value = "___ScriptCsValue1";
                _envVar2Value = "___ScriptCsValue2";
                SetEnvironmentVariables(_envVar1Key, _envVar1Value);
                SetEnvironmentVariables(_envVar2Key, _envVar2Value);
            }

            [Fact]
            public void ShouldReplaceEnvironmentVariables()
            {
                var line = @"#load ""%___ScriptCsKey1%\SomeText\%___ScriptCsKey2%""";

                var output = PreProcessorUtil.GetPath("#load", line);

                var expected = _envVar1Value + "\\SomeText\\" + _envVar2Value;
                output.ShouldEqual(expected);
            }

            private void SetEnvironmentVariables(string envVarKey, string envVarValue)
            {
                Environment.SetEnvironmentVariable(envVarKey, envVarValue);
            }

            public void Dispose()
            {
                SetEnvironmentVariables(_envVar1Key, string.Empty);
                SetEnvironmentVariables(_envVar2Key, string.Empty);
            }
        }
    }
}