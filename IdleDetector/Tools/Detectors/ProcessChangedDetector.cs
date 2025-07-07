using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace IdleDetector.Tools.Detectors
{
    internal class ProcessChangedDetector
    {
        public EventHandler<bool> OnBlacklistedProcessChanged;

        private List<ProcessInformation> WatchdogProcesses = new List<ProcessInformation>();
        private List<string> BlacklistedProcesses = new List<string>() { "taskmgr.exe", "perfmon.exe" };
        private string CreationQuery { get; } = @"SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";
        private string DeletionQuery { get; } = @"SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";
        private ManagementEventWatcher InstanceCreationWatcher { get; }
        private ManagementEventWatcher InstanceDeletionWatcher { get; }
        private int _BlacklistedProcessesRunning { get; set; } = 0;
        private int BlacklistedProcessesRunning
        {
            get
            {
                return _BlacklistedProcessesRunning;
            }
            set
            {
                if (value < 0)
                    _BlacklistedProcessesRunning = 0;
                else
                    _BlacklistedProcessesRunning = value;
            }
        }
        private bool IsBlacklistedProcessRunning { get; set; }

        private void OnInstanceCreation(object sender, EventArrivedEventArgs e)
        {
            using (ManagementBaseObject obj = (ManagementBaseObject)e.NewEvent["TargetInstance"])
            {
                string openedProcessFullName = ((string)obj["Name"]).ToLower();
                foreach (string item in BlacklistedProcesses)
                {
                    if (openedProcessFullName == item)
                    {
                        BlacklistedProcessesRunning++;
                        if (BlacklistedProcessesRunning == 1 && IsBlacklistedProcessRunning == false)
                        {
                            IsBlacklistedProcessRunning = true;
                            OnBlacklistedProcessesChanged();
                        }
                    }
                }
            }
        }
        private void OnInstanceDeletion(object sender, EventArrivedEventArgs e)
        {
            using (ManagementBaseObject obj = (ManagementBaseObject)e.NewEvent["TargetInstance"])
            {
                string openedProcessFullName = ((string)obj["Name"]).ToLower();
                foreach(string item in BlacklistedProcesses)
                {
                    if(openedProcessFullName == item)
                    {
                        BlacklistedProcessesRunning--;
                        if (BlacklistedProcessesRunning == 0 && IsBlacklistedProcessRunning == true)
                        {
                            IsBlacklistedProcessRunning = false;
                            OnBlacklistedProcessesChanged();
                        }
                    }
                }
            }
        }

        private void OnBlacklistedProcessesChanged()
        {
            EventHandler<bool> handler = OnBlacklistedProcessChanged;
            if (handler != null)
            {
                handler(this, IsBlacklistedProcessRunning);
            }
        }

        public ProcessChangedDetector()
        {
            InstanceCreationWatcher = new ManagementEventWatcher(CreationQuery);
            InstanceDeletionWatcher = new ManagementEventWatcher(DeletionQuery);
            InstanceCreationWatcher.EventArrived += new EventArrivedEventHandler(OnInstanceCreation);
            InstanceDeletionWatcher.EventArrived += new EventArrivedEventHandler(OnInstanceDeletion);
        }

        public void Initialize()
        {
            foreach (string item in BlacklistedProcesses)
            {
                if (Process.GetProcessesByName(item.Split('.')[0]).Length != 0)
                {
                    IsBlacklistedProcessRunning = true;
                    BlacklistedProcessesRunning++;
                }
            }
            if (IsBlacklistedProcessRunning)
            {
                OnBlacklistedProcessesChanged();
            }

            InstanceCreationWatcher.Start();
            InstanceDeletionWatcher.Start();
        }
    }

    internal class ProcessInformation
    {
        public string ProcessName { get; set; }
        public string ProcessAbsolutePath { get; set; }

        public ProcessInformation(string _ProcessName, string _ProcessAbsolutePath)
        {
            ProcessName = _ProcessName;
            ProcessAbsolutePath = _ProcessAbsolutePath;
        }
    }
}
