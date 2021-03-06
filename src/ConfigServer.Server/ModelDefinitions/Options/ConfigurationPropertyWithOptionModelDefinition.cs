﻿using System;
using System.Collections.Generic;

namespace ConfigServer.Server
{
    internal class ConfigurationPropertyWithOptionModelDefinition<TConfigSet, TOption> : ConfigurationPropertyWithOptionModelDefinition
    {
        readonly Func<TConfigSet, OptionSet<TOption>> optionProvider;
        readonly string optionPath;
        readonly ConfigurationDependency[] dependency;

        internal ConfigurationPropertyWithOptionModelDefinition(Func<TConfigSet, OptionSet<TOption>> optionProvider, string optionPath, string propertyName, Type propertyParentType) : base(propertyName, typeof(TConfigSet), typeof(TOption), propertyParentType)
        {
            this.optionProvider = optionProvider;
            this.optionPath = optionPath;
            dependency = new[] { new ConfigurationDependency(typeof(TConfigSet), optionPath) };
        }

        public override IEnumerable<ConfigurationDependency> GetDependencies() => dependency;

        public override IOptionSet GetOptionSet(object configurationSet)
        {
            var castedConfigurationSet = (TConfigSet)configurationSet;
            return optionProvider(castedConfigurationSet);
        }

    }

    internal abstract class ConfigurationPropertyWithOptionModelDefinition : ConfigurationPropertyModelBase, IOptionPropertyDefinition
    {
        public ConfigurationPropertyWithOptionModelDefinition(string propertyName,Type configurationSet, Type optionType, Type propertyParentType) : base(propertyName, optionType, propertyParentType)
        {
            ConfigurationSetType = configurationSet;
        }

        public Type ConfigurationSetType { get; }

        public abstract IOptionSet GetOptionSet(object configurationSet);

    }
}
