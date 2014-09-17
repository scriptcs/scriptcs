using System;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.NRefactory.CSharp;

namespace ScriptCs.Engine.Mono.Segmenter.Analyser.Visitors
{
    internal class MethodVisitor : DepthFirstAstVisitor
    {
        private readonly List<MethodVisitorResult> _methods;

        internal MethodVisitor()
        {
            _methods = new List<MethodVisitorResult>();
        }

        internal IList<MethodVisitorResult> GetMethodDeclarations()
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
                // method has no method body
                return;
            }
            methodBody = (BlockStatement)methodBody.Clone();

            var prototype = new VariableDeclarationStatement { Type = methodType };
            prototype.Variables.Add(new VariableInitializer(methodName));

            var anonymousMethod = new AnonymousMethodExpression(methodBody, parameters) { IsAsync = isAsync };
            var expression = new ExpressionStatement
            {
                Expression = new AssignmentExpression(
                    new IdentifierExpression(methodName), 
                    anonymousMethod)
            };

            _methods.Add(new MethodVisitorResult
            {
                MethodDefinition = methodDeclaration,
                MethodPrototype = prototype,
                MethodExpression = expression
            });
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
