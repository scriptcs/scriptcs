using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs.Contracts
{
    public interface IScriptRuntimeBuilder
    {
        ScriptRuntime Build();
        IScriptRuntimeBuilder Debug(bool debug = true);
        IScriptRuntimeBuilder ScriptName(string name);
        IScriptRuntimeBuilder Repl(bool repl = true);
        IScriptRuntimeBuilder LogLevel(LogLevel level);
        IScriptRuntimeBuilder ScriptExecutor<T>() where T : IScriptExecutor;
        IScriptRuntimeBuilder ScriptEngine<T>() where T : IScriptEngine;
        IScriptRuntimeBuilder ScriptHostFactory<T>() where T : IScriptHostFactory;
        IScriptRuntimeBuilder ScriptPackManager<T>() where T : IScriptPackManager;
        IScriptRuntimeBuilder ScriptPackResolver<T>() where T : IScriptPackResolver;
        IScriptRuntimeBuilder InstallationProvider<T>() where T : IInstallationProvider;
        IScriptRuntimeBuilder FileSystem<T>() where T : IFileSystem;
        IScriptRuntimeBuilder AssemblyUtility<T>() where T : IAssemblyUtility;
        IScriptRuntimeBuilder PackageContainer<T>() where T : IPackageContainer;
        IScriptRuntimeBuilder FilePreProcessor<T>() where T : IFilePreProcessor;
        IScriptRuntimeBuilder PackageAssemblyResolver<T>() where T : IPackageAssemblyResolver;
        IScriptRuntimeBuilder AssemblyResolver<T>() where T : IFilePreProcessor;
    }
}
