﻿using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distrib.Plugins
{
    public interface IPluginAssembly
    {
        IPluginAssemblyInitialisationResult Initialise();
        void Unitialise();
        bool IsInitialised { get; }

        IPluginInstance CreatePluginInstance(IPluginDescriptor descriptor);
    }
}