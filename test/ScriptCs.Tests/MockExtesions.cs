using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;

namespace ScriptCs.Tests
{
    public static class MockExtesions
    {
        public static ISetupGetter<TMocked, TProperty> SetupGet<TMocked, TProperty>(this TMocked obj,
            Expression<Func<TMocked, TProperty>> expression) where TMocked : class
        {
            return Mock.Get(obj).SetupGet(expression);
        }

        public static ISetup<TMocked, TResult> Setup<TMocked, TResult>(this TMocked obj,
            Expression<Func<TMocked, TResult>> expression) where TMocked : class
        {
            return Mock.Get(obj).Setup(expression);
        }

        public static void Verify<TMocked>(this TMocked obj, Expression<Action<TMocked>> expression, Times times)
            where TMocked : class
        {
            Mock.Get(obj).Verify(expression, times);
        }
    }
}