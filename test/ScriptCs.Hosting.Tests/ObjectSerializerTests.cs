using System;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class ObjectSerializerTests
    {
        public class TheSerializeMethod
        {
            private readonly ObjectSerializer _serializer;

            public TheSerializeMethod()
            {
                _serializer = new ObjectSerializer();
            }

            [Fact]
            public void ShouldSerialize()
            {
                // arrange
                var obj = new Foo
                {
                    Bar = new Bar
                    {
                        Baz = true,
                        Bazz = 123.4,
                        Bazzz = "hello",
                    },
                };

                // act
                var result = _serializer.Serialize(obj);

                // assert
                result.ShouldEqual(
@"{
  ""Bar"": {
    ""Baz"": true,
    ""Bazz"": 123.4,
    ""Bazzz"": ""hello""
  }
}");
            }

            [Fact]
            public void ShouldSerializeTypeMethods()
            {
                // arrange
                var obj = typeof(Type).GetMethods();

                // act
                var exception = Record.Exception(() => _serializer.Serialize(obj));

                // assert
                exception.ShouldBeNull();
            }

            [Fact]
            public void ShouldSerializeDelegates()
            {
                // arrange
                var obj = new FuncAndAction
                {
                    Action = () => { },
                    Func = () => "Hello World",
                };

                // act
                var exception = Record.Exception(() => _serializer.Serialize(obj));

                // assert
                exception.ShouldBeNull();
            }

            [Fact]
            public void ShouldSerializeWithCircularReferences()
            {
                // arrange
                var obj = new Circular();
                obj.Parent = obj;

                // act
                var result = _serializer.Serialize(obj);

                // assert
                result.ShouldEqual(
@"{
  ""$id"": ""1"",
  ""Parent"": {
    ""$ref"": ""1""
  }
}");
            }

            private class Foo
            {
                public Bar Bar { get; set; }
            }

            private class Bar
            {
                public bool Baz { get; set; }
                public double Bazz { get; set; }
                public string Bazzz { get; set; }
            }

            private class FuncAndAction
            {
                public Action Action { get; set; }
                public Func<string> Func { get; set; }
            }

            private class Circular
            {
                public Circular Parent { get; set; }
            }
        }
    }
}
