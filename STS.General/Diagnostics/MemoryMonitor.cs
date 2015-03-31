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

        public MemoryMonitor(int monitorPeriodInMilliseconds = 500)
            : this(true, true, true, monitorPeriodInMilliseconds)
        {
        }

        private void DoMonitor(object state)
        {
            CurrentProcess.Refresh();

            if (monitorPagedMemory)
            {
                CurrentPagedMemory = CurrentProcess.PagedMemorySize64;

                if (CurrentPagedMemory > PeakPagedMemory)
                    PeakPagedMemory = CurrentPagedMemory;
            }

            if (monitorWorkingSet)
            {
                CurrentWorkingSet = CurrentProcess.WorkingSet64;

                if (CurrentWorkingSet > PeakWorkingSet)
                    PeakWorkingSet = CurrentWorkingSet;
            }

            if (monitorVirtualMemory)
            {
                CurrentVirtualMemory = CurrentProcess.VirtualMemorySize64;

                if (CurrentVirtualMemory > PeakVirtualMemory)
                    PeakVirtualMemory = CurrentVirtualMemory;
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
            PeakPagedMemory = 0;
            PeakWorkingSet = 0;
            PeakVirtualMemory = 0;

            CurrentPagedMemory = 0;
            CurrentWorkingSet = 0;
            CurrentVirtualMemory = 0;
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

        public long CurrentPagedMemory { get; set; }
        public long CurrentWorkingSet { get; set; }
        public long CurrentVirtualMemory { get; set; }

        /// <summary>
        /// Shows the maximum amount of virtual memory, in bytes, that a process has reserved for use in the paging file(s). 
        /// Paging files are used to store pages of memory used by the process. Paging files are shared by all processes, 
        /// and the lack of space in paging files can prevent other processes from allocating memory. 
        /// If there is no paging file, this counter reflects the maximum amount of virtual memory that the process has reserved for use in physical memory.
        /// </summary>
        public long PeakPagedMemory { get; private set; }

        /// <summary>
        /// Shows the maximum size, in bytes, in the working set of this process. The working set is the set of memory pages that were touched recently by the threads in the process.
        /// If free memory in the computer is above a certain threshold, pages are left in the working set of a process, even if they are not in use. When free memory falls below a certain threshold, pages are trimmed from working sets. 
        /// If the pages are needed, they will be soft-faulted back into the working set before leaving main memory.
        /// </summary>
        public long PeakWorkingSet { get; private set; }

        /// <summary>
        /// Shows the maximum size, in bytes, of virtual address space that the process has used at any one time. 
        /// Use of virtual address space does not necessarily imply corresponding use of either disk or main memory pages. 
        /// However, virtual space is finite, and the process might limit its ability to load libraries by using too much.
        /// </summary>
        public long PeakVirtualMemory { get; private set; }
    }
}
