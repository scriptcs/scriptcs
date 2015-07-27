using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;
using Autofac;

namespace ScriptCs.Hosting
{
    public abstract class ScriptServicesRegistration
    {
        private readonly ILogProvider _logProvider;
        private readonly ILog _log;
        private readonly IDictionary<Type, object> _overrides;

        public ILogProvider LogProvider
        {
            get { return _logProvider; }
        }

        protected ScriptServicesRegistration(ILogProvider logProvider, IDictionary<Type, object> overrides)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);

            _overrides = overrides ?? new Dictionary<Type, object>();
            _logProvider = logProvider;
            _log = _logProvider.ForCurrentType();
        }

        protected void RegisterOverrideOrDefault<T>(ContainerBuilder builder, Action<ContainerBuilder> registrationAction)
        {
            Guard.AgainstNullArgument("registrationAction", registrationAction);

            if (_overrides.ContainsKey(typeof(T)))
            {
                var reg = _overrides[typeof(T)];
                _log.Debug(string.Format("Registering override: {0}", reg));

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
                _log.Debug(string.Format("Registering default: {0}", typeof(T)));
                registrationAction(builder);
            }
        }

        protected void RegisterLineProcessors(ContainerBuilder builder)
        {
            object processors;
            this.Overrides.TryGetValue(typeof(ILineProcessor), out processors);
            var processorList = (processors as IEnumerable<Type> ?? Enumerable.Empty<Type>()).ToArray();

            var loadProcessorType = processorList
                .FirstOrDefault(x => typeof(ILoadLineProcessor).IsAssignableFrom(x))
                ?? typeof(LoadLineProcessor);

            var usingProcessorType = processorList
                .FirstOrDefault(x => typeof(IUsingLineProcessor).IsAssignableFrom(x))
                ?? typeof(UsingLineProcessor);

            var referenceProcessorType = processorList
                .FirstOrDefault(x => typeof(IReferenceLineProcessor).IsAssignableFrom(x))
                ?? typeof(ReferenceLineProcessor);

            var shebangProcessorType = processorList
                .FirstOrDefault(x => typeof(IShebangLineProcessor).IsAssignableFrom(x))
                ?? typeof(ShebangLineProcessor);

            var processorArray = new[] { loadProcessorType, usingProcessorType, referenceProcessorType, shebangProcessorType }
                .Union(processorList).ToArray();

            builder.RegisterTypes(processorArray).As<ILineProcessor>();
        }

        private IContainer _container;

        public IContainer Container
        {
            get { return _container ?? (_container = CreateContainer()); }
        }

        protected IDictionary<Type, object> Overrides
        {
            get { return _overrides; }
        }

        protected abstract IContainer CreateContainer();
    }
}
