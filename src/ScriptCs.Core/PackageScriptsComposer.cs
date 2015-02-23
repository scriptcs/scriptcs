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
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        public PackageScriptsComposer(IFileSystem fileSystem, IFilePreProcessor preProcessor, IPackageContainer packageContainer, IPackageAssemblyResolver packageAssemblyResolver)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("preProcessor", preProcessor);
            Guard.AgainstNullArgument("packageContainer", packageContainer);
            Guard.AgainstNullArgument("packageAssemblyResolver", packageAssemblyResolver);

            _fileSystem = fileSystem;
            _preProcessor = preProcessor;
            _packageContainer = packageContainer;
            _packageAssemblyResolver = packageAssemblyResolver;
        }

        internal static string GetPackageScript(IPackageObject package)
        {
            var script =
                package.GetContentFiles()
                    .SingleOrDefault(f => f.EndsWith("main.csx", StringComparison.InvariantCultureIgnoreCase));

            return script;
        }

        public virtual string PackageScriptsFile
        {
            get { return "PackageScripts.csx"; }
        }

        public void Compose(StringBuilder builder = null)
        {
            var namespaces = new List<string>();
            var references = new List<string>();

            var packagesPath = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFolder);
            var packageReferences = _packageAssemblyResolver.GetPackages(_fileSystem.CurrentDirectory);
            var packageScriptsPath = Path.Combine(packagesPath, PackageScriptsFile);

            if (!_fileSystem.DirectoryExists(packagesPath) || _fileSystem.FileExists(packageScriptsPath))
            {
                return;
            }

            if (builder == null)
            {
                builder = new StringBuilder();
            }

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
            _fileSystem.WriteToFile(packageScriptsPath, builder.ToString());
        }

        protected internal virtual void ProcessPackage(string packagesPath, IPackageReference reference, StringBuilder builder, List<string> references,
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
