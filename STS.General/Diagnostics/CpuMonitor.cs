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
    public class CpuMonitor
    {
        private Timer timer;

        private PerformanceCounter privilegedTimeCounter;
        private PerformanceCounter processorTimeCounter;
        private PerformanceCounter userTimeCounter;

        private bool monitorPrivilegedTime;
        private bool monitorProcessorTime;
        private bool monitorUserTime;

        private int monitorPeriodInMilliseconds;

        public CpuMonitor(bool monitorPrivilegedTime, bool monitorProcessorTime, bool monitorUserTime, int monitorPeriodInMilliseconds = 500)
        {
            if (!monitorPrivilegedTime && !monitorProcessorTime && !monitorUserTime)
                throw new ArgumentException("At least one flag has to be true.");

            this.monitorPrivilegedTime = monitorPrivilegedTime;
            this.monitorProcessorTime = monitorProcessorTime;
            this.monitorUserTime = monitorUserTime;

            this.monitorPeriodInMilliseconds = monitorPeriodInMilliseconds;

            string processName = Process.GetCurrentProcess().ProcessName;

            if (monitorPrivilegedTime)
                privilegedTimeCounter = new PerformanceCounter("Process", "% Privileged Time", processName);
            if (monitorProcessorTime)
                processorTimeCounter = new PerformanceCounter("Process", "% Processor Time", processName);
            if (monitorUserTime)
                userTimeCounter = new PerformanceCounter("Process", "% User Time", processName);

            timer = new Timer(DoMonitor, null, Timeout.Infinite, MonitorPeriodInMilliseconds);
        }

        public CpuMonitor(int monitorPeriodInMilliseconds = 1000)
            : this(true, true, true, monitorPeriodInMilliseconds)
        {
        }

        private void DoMonitor(object state)
        {
            if (monitorPrivilegedTime)
            {
                PrivilegedTime = privilegedTimeCounter.NextValue() / System.Environment.ProcessorCount;

                if (PrivilegedTime > PeakPrivilegedTime)
                    PeakPrivilegedTime = PrivilegedTime;
            }

            if (monitorProcessorTime)
            {
                ProcessorTime = processorTimeCounter.NextValue() / System.Environment.ProcessorCount;

                if (ProcessorTime > PeakProcessorTime)
                    PeakProcessorTime = ProcessorTime;
            }

            if (monitorUserTime)
            {
                UserTime = userTimeCounter.NextValue() / System.Environment.ProcessorCount;

                if (UserTime > PeakUserTime)
                    PeakUserTime = UserTime;
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
            PrivilegedTime = 0;
            ProcessorTime = 0;
            UserTime = 0;

            PeakPrivilegedTime = 0;
            PeakProcessorTime = 0;
            PeakUserTime = 0;
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
        public float PrivilegedTime { get; private set; }

        /// <summary>
        /// Shows the percentage of time that the processor spent executing a non-idle thread. 
        /// It is calculated by measuring the duration that the idle thread is active during the sample interval, 
        /// and subtracting that time from 100 %. (Each processor has an idle thread that consumes cycles when no other threads are ready to run.)
        /// </summary>
        public float ProcessorTime { get; private set; }

        /// <summary>
        /// Shows the percentage of time that the processor spent executing code in user mode. 
        /// Applications, environment subsystems, and integral subsystems execute in user mode. 
        /// Code executing in user mode cannot damage the integrity of the Windows Executive, kernel, and/or device drivers.
        /// </summary>
        public float UserTime { get; private set; }

        /// <summary>
        /// Gets the peak privileged time value.
        /// </summary>
        public float PeakPrivilegedTime { get; private set; }

        /// <summary>
        /// Gets the peak processor time value.
        /// </summary>
        public float PeakProcessorTime { get; private set; }

        /// <summary>
        /// Gets the peak user time value.
        /// </summary>
        public float PeakUserTime { get; private set; }
    }
}
