namespace ScriptCs.Engine.Mono.Parser.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ICSharpCode.NRefactory.CSharp;

    internal class MethodVisitor : DepthFirstAstVisitor
    {
        private readonly List<Tuple<MethodDeclaration, FieldDeclaration>> _methods;

        internal MethodVisitor()
        {
            _methods = new List<Tuple<MethodDeclaration, FieldDeclaration>>();
        }

        internal List<Tuple<MethodDeclaration, FieldDeclaration>> GetMethodDeclarations()
        {
            return _methods;
        }

        public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        {
            Guard.AgainstNullArgument("methodDeclaration", methodDeclaration);

            IEnumerable<ParameterDeclaration> parameters = methodDeclaration
                .GetChildrenByRole(Roles.Parameter)
                .Select(x => (ParameterDeclaration)x.Clone());

            var isVoid = false;
            var isAsync = methodDeclaration.Modifiers.HasFlag(Modifiers.Async);
            AstType returnType = methodDeclaration.GetChildByRole(Roles.Type).Clone();
            var type = returnType as PrimitiveType;
            if (type != null)
            {
                isVoid = string.Compare(
                    type.Keyword, "void", StringComparison.OrdinalIgnoreCase) == 0;
            }

            var methodType = new SimpleType(Identifier.Create(isVoid ? "Action" : "Func"));

            IEnumerable<AstType> types = parameters.Select(
                x => x.GetChildByRole(Roles.Type).Clone());

            methodType
                .TypeArguments
                .AddRange(types);

            if (!isVoid)
            {
                methodType.TypeArguments.Add(returnType);
            }
            var methodName = GetIdentifierName(methodDeclaration);

            var methodBody = methodDeclaration
                .GetChildrenByRole(Roles.Body)
                .FirstOrDefault();
            if (methodBody == null)
            {
                throw new NullReferenceException(string.Format("Method '{0}' has no method body", methodName));
            }
            methodBody = (BlockStatement)methodBody.Clone();

            var anonymousMethod = new AnonymousMethodExpression(methodBody, parameters) { IsAsync = isAsync };
            var methodExpression = new VariableInitializer(methodName, anonymousMethod);

            var namedMethodExpr = new FieldDeclaration { ReturnType = methodType };
            namedMethodExpr.Variables.Add(methodExpression);

            _methods.Add(new Tuple<MethodDeclaration, FieldDeclaration>(methodDeclaration, namedMethodExpr));
        }

        private static string GetIdentifierName(AstNode node)
        {
            foreach (var obj in
                from child in node.GetChildrenByRole(Roles.Identifier)
                from propertyInfo in child
                    .GetType()
                    .GetProperties(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public)
                    .Where(x => x.Name == "Name")
                select propertyInfo.GetValue(child, null))
            {
                return obj.ToString();
            }
            throw new MissingFieldException("Missing Role 'Identifier' from AstNode");
        }
    }
}
