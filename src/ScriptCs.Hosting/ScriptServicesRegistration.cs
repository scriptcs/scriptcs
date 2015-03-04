using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;
using Autofac;
using Common.Logging;

namespace ScriptCs.Hosting
{
    public abstract class ScriptServicesRegistration
    {
        private readonly IDictionary<Type, object> _overrides;

        public ILog Logger { get; private set; }

        protected ScriptServicesRegistration(ILog logger, IDictionary<Type, object> overrides)
        {
            _overrides = overrides ?? new Dictionary<Type, object>();
            Logger = logger;
        }

        protected void RegisterOverrideOrDefault<T>(ContainerBuilder builder, Action<ContainerBuilder> registrationAction)
        {
            Guard.AgainstNullArgument("registrationAction", registrationAction);

            if (_overrides.ContainsKey(typeof(T)))
            {
                var reg = _overrides[typeof(T)];
                this.Logger.Debug(string.Format("Registering override: {0}", reg));

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
                this.Logger.Debug(string.Format("Registering default: {0}", typeof(T)));
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

            var processorArray = new[] { loadProcessorType, usingProcessorType, referenceProcessorType }
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
