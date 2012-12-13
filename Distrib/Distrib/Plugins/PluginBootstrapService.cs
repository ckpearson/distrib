﻿using Distrib.Utils;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distrib.Plugins
{
    public sealed class PluginBootstrapService : IPluginBootstrapService
    {
        private readonly IPluginControllerFactory _pluginControllerFactory;

        public PluginBootstrapService(IPluginControllerFactory pluginControllerFactory)
        {
            _pluginControllerFactory = pluginControllerFactory;
        }

        public Res<PluginBootstrapResult> BootstrapPlugin(IPluginDescriptor descriptor)
        {
            PluginBootstrapResult res = PluginBootstrapResult.Success;

            if (descriptor == null) throw new ArgumentNullException("Descriptor must be supplied");

            try
            {
                // If descriptor has no controller type set, then set the current default
                if (descriptor.Metadata.ControllerType == null)
                {
                    descriptor.Metadata.ControllerType = _pluginControllerFactory
                        .CreateController().GetType();
                }

                return new Res<PluginBootstrapResult>(res == PluginBootstrapResult.Success, res);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to bootstrap plugin", ex);
            }
        }
    }
}
