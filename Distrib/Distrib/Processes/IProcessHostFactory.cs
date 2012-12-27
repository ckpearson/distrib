﻿using Distrib.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distrib.Processes
{
    public interface IProcessHostFactory
    {
        IProcessHost CreateHostForProcessPlugin(IPluginInstance pluginInstance);
    }
}
