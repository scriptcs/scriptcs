using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public abstract class ScriptContainerFactory 
    {
        private readonly IDictionary<Type, object> _overrides = null;
        protected readonly ILog _logger;

        public ScriptContainerFactory(ILog logger, IDictionary<Type, object> overrides)
        {
            _overrides = overrides;
            _logger = logger;
        }

        protected void RegisterOverrideOrDefault<T>(ContainerBuilder builder, Action<ContainerBuilder> registrationAction)
        {
            if (_overrides.ContainsKey(typeof(T)))
            {
                var reg = _overrides[typeof(T)];
                if (reg.GetType().IsSubclassOf(typeof(Type)))
                {
                    builder.RegisterType((Type)reg).As<T>().SingleInstance();
                }
                else
                {
                    builder.RegisterInstance(reg).As<T>();
                }
            }
            else
            {
                registrationAction(builder);
            }
        }

        private IContainer _container;
        public IContainer Container
        {
            get
            {
                if (_container == null)
                    _container = CreateContainer();

                return _container;
            }
        }

        protected abstract IContainer CreateContainer();

    }
}
