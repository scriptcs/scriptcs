using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;
using System.IO;

namespace ScriptCs
{
    public class PackageScriptsComposer : IPackageScriptsComposer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFilePreProcessor _preProcessor;
        private readonly IPackageContainer _packageContainer;

        public PackageScriptsComposer(IFileSystem fileSystem, IFilePreProcessor preProcessor, IPackageContainer packageContainer)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("preProcessor", preProcessor);
            Guard.AgainstNullArgument("packageContainer", packageContainer);

            _fileSystem = fileSystem;
            _preProcessor = preProcessor;
            _packageContainer = packageContainer;
        }

        internal static string GetPackageScript(IPackageObject package)
        {
            var script =
                package.GetContentFiles()
                    .SingleOrDefault(f => f.EndsWith("pack.csx", StringComparison.InvariantCultureIgnoreCase));

            return script;
        }

        public void Compose(IEnumerable<IPackageReference> packageReferences, StringBuilder builder = null)
        {
            var namespaces = new List<string>();
            var references = new List<string>();

            if (builder == null)
            {
                builder = new StringBuilder();
            }

            var packagesPath = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFolder);
            foreach (var reference in packageReferences)
            {
                ProcessPackage(packagesPath, reference, builder, references, namespaces);
            }

            foreach (var ns in namespaces)
            {
                builder.Insert(0, String.Format("using {0};{1}", ns, Environment.NewLine));
            }

            foreach (var reference in references)
            {
                builder.Insert(0, String.Format("#r {0}{1}", reference, Environment.NewLine));
            }
            var outputPath = Path.Combine(packagesPath, _fileSystem.PackageScriptsFile);
            _fileSystem.WriteToFile(outputPath, builder.ToString());
        }

        internal protected virtual void ProcessPackage(string packagesPath, IPackageReference reference, StringBuilder builder, List<string> references,
            List<string> namespaces)
        {
            var package = _packageContainer.FindPackage(packagesPath, reference);
            var script = GetPackageScript(package);
            if (script != null)
            {
                script = Path.Combine(packagesPath, string.Format("{0}.{1}", package.Id, package.TextVersion), script);
                var result = _preProcessor.ProcessFile(script);
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(script);
                var classname = fileWithoutExtension.Substring(0, fileWithoutExtension.Length - 4);
                builder.AppendFormat("public class {0} : ScriptCs.PackageScriptWrapper {{{1}", classname, Environment.NewLine);
                builder.AppendLine(result.Code);
                builder.Append("}");
                references.AddRange(result.References);
                namespaces.AddRange(result.Namespaces);
            }
        }
    }
}
