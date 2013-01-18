﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distrib.Plugins
{
    /// <summary>
    /// Represents the link to the plugin framework given to plugins
    /// </summary>
    public interface IPluginInteractionLink
    {
        /// <summary>
        /// Gets the metadata for the plugin
        /// </summary>
        IPluginMetadata PluginMetadata { get; }

        /// <summary>
        /// Gets the additional metadata bundles that the plugin has
        /// </summary>
        IReadOnlyList<IPluginMetadataBundle> AdditionalMetadataBundles { get; }

        /// <summary>
        /// Gets the creation stamp for when the actual instance of the plugin class was created
        /// </summary>
        DateTime PluginCreationStamp { get; }

        /// <summary>
        /// Registers with the plugin system and host a given assembly file as a dependency that needs
        /// to accompany the plugin assembly
        /// </summary>
        /// <param name="location">The location of the assembly</param>
        void RegisterDependentAssembly(string location);

        /// <summary>
        /// Get the list of assemblies that have been explicitly registered as dependencies
        /// </summary>
        IReadOnlyList<string> RegisteredDependentAssemblies { get; }
    }
}
