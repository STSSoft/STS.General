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
    /// Provides CPU usage monitoring for a process.
    /// </summary>
    public class ProcessorMonitor
    {
        private Timer timer;

        private PerformanceCounter privilegedTimeCounter;
        private PerformanceCounter processorTimeCounter;
        private PerformanceCounter userTimeCounter;

        private bool monitorPrivilegedTime;
        private bool monitorProcessorTime;
        private bool monitorUserTime;

        private long sampleCounter;

        private float totalPrivilegedTime;
        private float totalProcessorTime;
        private float totalUserTime;

        private float averagePrivilegedTime;
        private float averageProcessorTime;
        private float averageUserTime;

        private int monitorPeriodInMilliseconds;

        public ProcessorMonitor(bool monitorPagedMemory, bool monitorWorkingSet, bool monitorVirtualMemory, int monitorPeriodInMilliseconds = 500)
        {
            if (!monitorPagedMemory && !monitorWorkingSet && !monitorVirtualMemory)
                throw new ArgumentException("At least one flag has to be true.");

            this.monitorPrivilegedTime = monitorPagedMemory;
            this.monitorProcessorTime = monitorWorkingSet;
            this.monitorUserTime = monitorVirtualMemory;
            this.monitorPeriodInMilliseconds = monitorPeriodInMilliseconds;

            string processName = Process.GetCurrentProcess().ProcessName;

            if (monitorPagedMemory)
                privilegedTimeCounter = new PerformanceCounter("Process", "% Privileged Time", processName);
            if (monitorWorkingSet)
                processorTimeCounter = new PerformanceCounter("Process", "% Processor Time", processName);
            if (monitorVirtualMemory)
                userTimeCounter = new PerformanceCounter("Process", "% User Time", processName);

            timer = new Timer(DoMonitor, null, Timeout.Infinite, MonitorPeriodInMilliseconds);
        }

        public ProcessorMonitor(int monitorPeriodInMilliseconds = 1000)
            : this(true, true, true, monitorPeriodInMilliseconds)
        {
        }

        private void DoMonitor(object state)
        {
            sampleCounter++;

            if (monitorPrivilegedTime)
            {
                PrivilegTimePercent = privilegedTimeCounter.NextValue() / System.Environment.ProcessorCount;
                
                totalPrivilegedTime += PrivilegTimePercent;
                averagePrivilegedTime = totalUserTime / sampleCounter;
            }

            if (monitorProcessorTime)
            {
                ProcessorTimePercent = processorTimeCounter.NextValue() / System.Environment.ProcessorCount;

                totalProcessorTime += ProcessorTimePercent;
                averageProcessorTime = totalProcessorTime / sampleCounter;
            }

            if (monitorUserTime)
            {
                UserTimePercent = userTimeCounter.NextValue() / System.Environment.ProcessorCount;

                totalUserTime += UserTimePercent;
                averageUserTime = totalUserTime / sampleCounter;
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
            PrivilegTimePercent = 0;
            ProcessorTimePercent = 0;
            UserTimePercent = 0;

            sampleCounter = 0;

            totalPrivilegedTime = 0;
            totalProcessorTime = 0;
            totalUserTime = 0;

            averagePrivilegedTime = 0;
            averageProcessorTime = 0;
            averageUserTime = 0;
        }

        /// <summary>
        /// Gets or sets the interval between every measure.
        /// </summary>
        private int MonitorPeriodInMilliseconds
        {
            get { return monitorPeriodInMilliseconds;}
            set
            {
                monitorPeriodInMilliseconds = value;
                timer.Change(0, monitorPeriodInMilliseconds);
            }
        }

         /// <summary>
        /// Shows the percentage of non-idle processor time spent executing code in privileged mode. Privileged mode is a processing mode designed for 
        /// operating system components and hardware-manipulating drivers. 
        /// It allows direct access to hardware and memory. The alternative, user mode, is a restricted processing mode designed for applications, 
        /// environmental subsystems, and integral subsystems. The operating system switches application threads to privileged mode to access operating system services.
        /// </summary>
        public float PrivilegTimePercent { get; private set; }

        /// <summary>
        /// Shows the percentage of time that the processor spent executing a non-idle thread. 
        /// It is calculated by measuring the duration that the idle thread is active during the sample interval, 
        /// and subtracting that time from 100 %. (Each processor has an idle thread that consumes cycles when no other threads are ready to run.)
        /// </summary>
        public float ProcessorTimePercent { get; private set; }

        /// <summary>
        /// Shows the percentage of time that the processor spent executing code in user mode. 
        /// Applications, environment subsystems, and integral subsystems execute in user mode. 
        /// Code executing in user mode cannot damage the integrity of the Windows Executive, kernel, and/or device drivers.
        /// </summary>
        public float UserTimePercent { get; private set; }

        /// <summary>
        /// Gets the average PrivilegedTime.
        /// </summary>
        public float AveragePrivilegedTimePercent
        {
            get { return averagePrivilegedTime; }
        }

        /// <summary>
        /// Gets the average ProcessorTime.
        /// </summary>
        public float AverageProcessorTimePercent
        {
            get { return averageProcessorTime; }
        }

        /// <summary>
        /// Gets the average UserTime.
        /// </summary>
        public float AverageUserTimePercent
        {
            get { return averageUserTime; }
        }
    }
}
