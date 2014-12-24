using System;
using System.Collections.Generic;
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
