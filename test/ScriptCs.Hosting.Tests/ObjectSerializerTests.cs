using System;

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

            private class Foo
            {
                public Action Action { get; set; }
                public Func<string> Func { get; set; }
                public Foo Parent { get; set; }
            }
        }
    }
}