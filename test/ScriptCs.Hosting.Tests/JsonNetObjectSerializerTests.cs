using System;

using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class JsonNetObjectSerializerTests
    {
        public class TheSerializeMethod
        {
            private readonly JsonNetObjectSerializer _serializer;

            public TheSerializeMethod()
            {
                _serializer = new JsonNetObjectSerializer();
            }

            [Fact]
            public void ShouldSerializeTypeMethods()
            {
                Assert.DoesNotThrow(() => _serializer.Serialize(typeof(Type).GetMethods()));
            }

            [Fact]
            public void ShouldSerializeDelegates()
            {
                Assert.DoesNotThrow(() => _serializer.Serialize(new Action(() => { })));
                Assert.DoesNotThrow(() => _serializer.Serialize(new Foo
                {
                    Action = () => { },
                    Func = () => "Hello World"
                }));
            }

            [Fact]
            public void ShouldSerializeWithCircularReferences()
            {
                var foo = new Foo();
                foo.Parent = foo;

                Assert.DoesNotThrow(() => _serializer.Serialize(foo));
            }

            [Fact]
            public void ShouldNotContainIdProperty()
            {
                Assert.DoesNotContain("$id", _serializer.Serialize(new Foo()));
            }

            [Fact]
            public void ShouldNotContainRefProperty()
            {
                var foo = new Foo();
                foo.Parent = foo;

                Assert.DoesNotContain("$ref", _serializer.Serialize(foo));
            }

            private class Foo
            {
                public Action Action { get; set; }
                public Func<string> Func { get; set; }
                public Foo Parent { get; set; }
            }
        }
    }
}