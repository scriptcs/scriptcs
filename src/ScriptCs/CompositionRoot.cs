﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mef;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    using System.Linq;

    public class CompositionRoot
    {
        private static readonly Type _contractNameServices = typeof(ExportAttribute).Assembly.GetType("System.ComponentModel.Composition.ContractNameServices", true);
        private static readonly PropertyInfo _typeIdentityCache = _contractNameServices.GetProperty("TypeIdentityCache", BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.NonPublic);

        private readonly bool _debug;
        private IContainer _container;
        private ScriptServiceRoot _scriptServiceRoot;

        public CompositionRoot(bool debug)
        {
            _debug = debug;
        }

        public void Initialize()
        {
            var builder = new ContainerBuilder();
            var types = new[]
                {
                    typeof (ScriptHostFactory),
                    typeof (FileSystem),
                    typeof (PackageAssemblyResolver),
                    typeof (PackageContainer),
                    typeof (FilePreProcessor),
                    typeof (ScriptPackResolver),
                    typeof (NugetInstallationProvider),
                    typeof (PackageInstaller)
                };

            builder.RegisterTypes(types).AsImplementedInterfaces();

            if (_debug)
            {
                builder.RegisterType<DebugScriptExecutor>().As<IScriptExecutor>();
                builder.RegisterType<RoslynScriptDebuggerEngine>().As<IScriptEngine>();
            }
            else
            {
                builder.RegisterType<ScriptExecutor>().As<IScriptExecutor>();
                builder.RegisterType<RoslynScriptEngine>().As<IScriptEngine>();
            }

            builder.RegisterType<ScriptServiceRoot>().As<ScriptServiceRoot>();
            var scriptPath = Path.Combine(Environment.CurrentDirectory, "bin");
            if (Directory.Exists(scriptPath))
            {
                var catalog = new DirectoryCatalog(scriptPath);
                builder.RegisterComposablePartCatalog(
                    catalog,
                    ed =>
                        {
                            var ct = FindType(ed.ContractName);
                            if (ct != null) return new[] { new TypedService(ct) };

                            var et = FindType((string)ed.Metadata[CompositionConstants.ExportTypeIdentityMetadataName]);
                            if (et != null) return new[] { new KeyedService(ed.ContractName, et) };

                            return new Service[0];
                        });
            }
            _container = builder.Build();
            _scriptServiceRoot = _container.Resolve<ScriptServiceRoot>();
        }

        public ScriptServiceRoot GetServiceRoot()
        {
            return _scriptServiceRoot;
        }

        static Type FindType(string exportTypeIdentity)
        {
            var cache = (Dictionary<Type, string>)_typeIdentityCache.GetValue(null, null);
            return cache.FirstOrDefault(kvp => kvp.Value == exportTypeIdentity).Key;
        }
    }
}