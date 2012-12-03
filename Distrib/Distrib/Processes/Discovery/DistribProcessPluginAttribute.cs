﻿using Distrib.Plugins_old.Discovery;
using Distrib.Processes.Discovery.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distrib.Processes.Discovery
{
    /// <summary>
    /// Indicates that a given class is to be treated as a Distrib Process Plugin
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DistribProcessPluginAttribute : DistribPluginAttribute
    {
        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="name">The name of the process</param>
        /// <param name="description">The description of the process</param>
        /// <param name="version">The version of the process</param>
        /// <param name="author">The author of the process</param>
        public DistribProcessPluginAttribute(string name,
            string description,
            double version,
            string author,
            string identifier) : base(typeof(IDistribProcess), name, description, version, author, identifier)
        {
            base.SuppliedAdditionalMetadata = new List<IPluginAdditionalMetadataBundle>()
            {
                new DistribProcessDetailsAttribute(name, description, version, author).ToMetadataBundle(),
            }.AsReadOnly();
        }
    }
}