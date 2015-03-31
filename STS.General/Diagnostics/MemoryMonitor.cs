using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STS.General.Diagnostics
{
    /// <summary>
    /// Provides memory monitoring for a process.
    /// </summary>
    public class MemoryMonitor
    {
        private Timer timer;
        private Process CurrentProcess;

        private bool monitorPagedMemory;
        private bool monitorWorkingSet;
        private bool monitorVirtualMemory;

        private int monitorPeriodInMilliseconds;

        public MemoryMonitor(bool monitorPagedMemory, bool monitorWorkingSet, bool monitorVirtualMemory, int monitorPeriodInMilliseconds = 500)
        {
            if (!monitorPagedMemory && !monitorWorkingSet && !monitorVirtualMemory)
                throw new ArgumentException("At least one flag has to be true.");

            this.monitorPagedMemory = monitorPagedMemory;
            this.monitorWorkingSet = monitorWorkingSet;
            this.monitorVirtualMemory = monitorVirtualMemory;
            this.monitorPeriodInMilliseconds = monitorPeriodInMilliseconds;

            CurrentProcess = Process.GetCurrentProcess();

            timer = new Timer(DoMonitor, null, Timeout.Infinite, MonitorPeriodInMilliseconds);
        }

        public MemoryMonitor(int monitorPeriodInMilliseconds = 1000)
            : this(true, true, true, monitorPeriodInMilliseconds)
        {
        }

        private void DoMonitor(object state)
        {
            CurrentProcess.Refresh();

            if (monitorPagedMemory)
            {
                PagedMemory = CurrentProcess.PagedMemorySize64;

                if (PagedMemory > PeakPagedMemory)
                    PeakPagedMemory = PagedMemory;
            }

            if (monitorWorkingSet)
            {
                WorkingSet = CurrentProcess.WorkingSet64;

                if (WorkingSet > PeakWorkingSet)
                    PeakWorkingSet = WorkingSet;
            }

            if (monitorVirtualMemory)
            {
                VirtualMemory = CurrentProcess.VirtualMemorySize64;

                if (VirtualMemory > PeakVirtualMemory)
                    PeakVirtualMemory = VirtualMemory;
            }
        }

        public void Start()
        {
            timer.Change(0, MonitorPeriodInMilliseconds);
        }

        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Reset()
        {
            PagedMemory = 0;
            WorkingSet = 0;
            VirtualMemory = 0;

            PeakPagedMemory = 0;
            PeakWorkingSet = 0;
            PeakVirtualMemory = 0;
        }

        /// <summary>
        /// Gets or sets the interval between every measure.
        /// </summary>
        public int MonitorPeriodInMilliseconds
        {
            get { return monitorPeriodInMilliseconds; }
            set
            {
                monitorPeriodInMilliseconds = value;
                timer.Change(0, monitorPeriodInMilliseconds);
            }
        }

        /// <summary>
        /// The amount of memory, in bytes, the system has allocated for the associated
        //  process that can be written to the virtual memory paging file.
        /// </summary>
        public long PagedMemory { get; private set; }

        /// <summary>
        /// The amount of physical memory, in bytes, allocated for the associated process.
        /// </summary>
        public long WorkingSet { get; private set; }

        /// <summary>
        /// The total amount of physical memory the associated process is using, in bytes.
        /// </summary>
        public long VirtualMemory { get; private set; }

        /// <summary>
        /// Gets the peak paged memory value.
        /// </summary>
        public long PeakPagedMemory { get; private set; }

        /// <summary>
        /// Gets the peak working set value.
        /// </summary>
        public long PeakWorkingSet { get; private set; }

        /// <summary>
        /// Gets the peak virtual memory value.
        /// </summary>
        public long PeakVirtualMemory { get; private set; }
    }
}
