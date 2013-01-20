﻿/*
	This software known as 'Distrib' at time of creation is under the GNU GPL v2. License
		This license can be found at: http://www.gnu.org/licenses/gpl-2.0.html

	This software is the sole copyright of Clint Pearson
		Contact: clintkpearson@gmail.com

	These following points are generalisations of the terms of the license and as such you MUST read the license itself
	in order to correctly know your rights and responsibilities.

	Primarily the license states:
		> You ARE ALLOWED to use this software be it in binary form or in source form (whole or part) in any software that is
			1. Free to use (though you may charge for distribution)
			2. Free to study and modify (though you may charge for distribution of the source code)
			3. Available under the same license as this software

		> You ARE NOT ALLOWED to use this software be it in binary form or in source form (whole or part) in any software that is:
			1. Commercial (this covers any software for which there is a fee to use and procure)
			2. Software that is closed-source (even if the source is available for a fee)
			3. Not available under the same license as this software

	If this software has been made available to you under any license other than the original license by any party other than the
	original copyright holder (Clint Pearson) then they have acted under breach of their original agreement.

	If this software has been made available to you for a fee for distribution by any party other than the original copyright holder (Clint Pearson)
	then they have acted under breach of their original agreement unless this software is a derivative created by that party.

	If you have received this software from the original copyright holder (Clint Pearson) and it has been made available to you under
	the terms of the original license and you wish to obtain a different license to cover your use of the software, then you may contact
	the copyright holder to negotiate a new license.
*/
using Distrib.Plugins;
using Distrib.Processes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Ninject;

namespace ProcessTester.Model
{
    public sealed class PluginAssemblyModel : INotifyPropertyChanged
    {
        private readonly IPluginAssembly _assembly;

        public PluginAssemblyModel(IPluginAssembly pluginAsm)
        {
            _assembly = pluginAsm;
        }

        public bool Initialised
        {
            get { return _assembly.IsInitialised; }
        }

        private IProcessHost _procHost;
        public IProcessHost ProcessHost
        {
            get
            {
                return _procHost;
            }

            set
            {
                _procHost = value;
                _processInputs = null;
                _processOutputs = null;
                PropChange("ProcessHost");
                PropChange("HasProcessHost");
                PropChange("ProcessInputs");
                PropChange("ProcessOutputs");
            }
        }

        public bool HasProcessHost
        {
            get
            {
                return _procHost != null;
            }
        }

        private IPluginAssemblyInitialisationResult _initResult = null;

        public IReadOnlyList<IPluginDescriptor> Plugins
        {
            get
            {
                if (_initResult == null)
                    return null;
                else
                {
                    return _initResult.Plugins;
                }
            }
        }

        private bool _busy;
        public bool Busy
        {
            get { return _busy; }
            set
            {
                _busy = value;
                PropChange("Busy");
            }
        }

        private void DoAsBusy(Action act, Action whenDone = null)
        {
            Busy = true;
            var task = Task.Factory.StartNew(() =>
                {
                    act();
                }).ContinueWith(
                t =>
                {
                    Busy = false;
                    if (whenDone != null)
                    {
                        whenDone();
                    }
                });
        }

        public string Path
        {
            get { return _assembly.AssemblyFilePath; }
        }

        private RelayCommand _toggleInitCommand;
        public ICommand ToggleInitCommand
        {
            get
            {
                if (_toggleInitCommand == null)
                {
                    _toggleInitCommand = new RelayCommand((arg) =>
                        {
                            DoAsBusy(() =>
                                {
                                    if (_assembly.IsInitialised)
                                    {
                                        if (ProcessHost != null)
                                        {
                                            ProcessHost.Unitialise();
                                            ProcessHost = null;
                                        }
                                        _assembly.Unitialise();
                                        _initResult = null;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            _initResult = _assembly.Initialise();
                                        }
                                        catch (Exception ex)
                                        {
                                            if (_assembly.IsInitialised)
                                                _assembly.Unitialise();
                                        }
                                        PropChange("Plugins");
                                    }

                                    PropChange("Initialised");
                                });
                        });
                }

                return _toggleInitCommand;
            }
        }

        private IReadOnlyList<IProcessJobValueField> _processInputs;
        public IReadOnlyList<IProcessJobValueField> ProcessInputs
        {
            get
            {
                if (ProcessHost == null)
                {
                    _processInputs = null;
                    return null;
                }

                if (_processInputs == null)
                {
                    if (!ProcessHost.IsInitialised)
                    {
                        return null;
                    }

                    _processInputs = ProcessHost.JobDescriptor.InputFields.Select(f => ProcessJobFieldFactory.CreateValueField(f)).ToList().AsReadOnly();
                    foreach (var input in _processInputs)
                    {
                        if (input.Definition.Config.HasDefaultValue)
                        {
                            input.Value = input.Definition.Config.DefaultValue;
                        }
                    }
                }

                return _processInputs;
            }
        }

        private IReadOnlyList<IProcessJobValueField> _processOutputs;
        public IReadOnlyList<IProcessJobValueField> ProcessOutputs
        {
            get { return _processOutputs; }
            set
            {
                _processOutputs = value;
                PropChange("ProcessOutputs");
            }
        }

        private string _processExecutionError;
        public string ProcessExecutionError
        {
            get { return _processExecutionError; }
            set
            {
                _processExecutionError = value;
                PropChange("ProcessExecutionError");
            }
        }

        private RelayCommand _performJobCommand;
        public ICommand PerformJobCommand
        {
            get
            {
                if (_performJobCommand == null)
                {
                    _performJobCommand = new RelayCommand((a) =>
                        {
                            DoAsBusy(() =>
                                {
                                    if (ProcessHost != null && ProcessHost.IsInitialised)
                                    {
                                        lock (ProcessHost)
                                        {
                                            ProcessExecutionError = null;

                                            try
                                            {
                                                foreach (var input in ProcessInputs)
                                                {
                                                    if (input.Value != null)
                                                    {
                                                        try
                                                        {
                                                            input.Value = Convert.ChangeType(input.Value, input.Definition.Type);
                                                        }
                                                        catch (Exception)
                                                        {
                                                            ProcessExecutionError =
                                                                string.Format("Could not convert '{0}' to '{1}' for input '{2}'",
                                                                input.Value,
                                                                input.Definition.Type,
                                                                input.Definition.Name);
                                                            return;
                                                        }
                                                    }
                                                }
                                                ProcessOutputs = ProcessHost.ProcessJob(ProcessInputs).ToList().AsReadOnly();
                                            }
                                            catch (Exception ex)
                                            {
                                                ProcessExecutionError = ex.Message;
                                            }
                                        }
                                    }
                                });
                        },
                        (a) =>
                        {
                            if (ProcessHost == null)
                                return false;

                            if (ProcessHost.IsInitialised == false)
                                return false;

                            return true;
                        });
                }

                return _performJobCommand;
            }
        }

        private RelayCommand _createProcessHostCommand;
        public ICommand CreateProcessHostCommand
        {
            get
            {
                if (_createProcessHostCommand == null)
                {
                    _createProcessHostCommand = new RelayCommand((arg) =>
                        {
                            var pd = arg as IPluginDescriptor;
                            if (pd == null)
                            {
                                throw new InvalidOperationException();
                            }

                            if (!pd.IsUsable)
                            {
                                throw new InvalidOperationException();
                            }

                            if (!pd.Metadata.InterfaceType.Equals(typeof(IProcess)))
                            {
                                throw new InvalidOperationException();
                            }

                            if (ProcessHost != null)
                            {
                                DoAsBusy(() =>
                                    {
                                        if (ProcessHost.IsInitialised)
                                        {
                                            ProcessHost.Unitialise();
                                        }
                                    }, () =>
                                        {
                                            ProcessHost = null;
                                        });
                            }

                            DoAsBusy(() =>
                                {
                                    ProcessHost = IOC.Kernel.Get<IProcessHostFactory>()
                                        .CreateHostFromPluginSeparated(pd);
                                }, () =>
                                    {
                                        DoAsBusy(() =>
                                            {
                                                ProcessHost.Initialise();

                                            }, () =>
                                            {
                                                PropChange("ProcessInputs");
                                            });
                                    });
                        },
                        (arg) =>
                        {
                            var pd = arg as IPluginDescriptor;
                            if (pd == null)
                            {
                                return false;
                            }

                            if (!pd.IsUsable)
                            {
                                return false;
                            }

                            return pd.Metadata.InterfaceType.Equals(typeof(IProcess));
                        });
                }

                return _createProcessHostCommand;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void PropChange(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
