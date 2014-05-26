namespace ScriptCs.Engine.Mono.Parser.Visitors
{
    using System.Collections.Generic;

    using ICSharpCode.NRefactory.CSharp;

    internal class ClassTypeVisitor : DepthFirstAstVisitor
    {
        private readonly List<TypeDeclaration> _classes;

        internal ClassTypeVisitor()
        {
            _classes = new List<TypeDeclaration>();
        }

        internal List<TypeDeclaration> GetClassDeclarations()
        {
            return _classes;
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            _classes.Add(typeDeclaration);
            base.VisitTypeDeclaration(typeDeclaration);
        }
    }
}
