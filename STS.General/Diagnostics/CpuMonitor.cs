using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if NETFX_CORE 
using Windows.System.Diagnostics;
#endif

namespace STS.General.Diagnostics
{
    /// <summary>
    /// Provides CPU usage monitoring for a process.
    /// </summary>
    public class CPUMonitor
    {
        private Timer timer;

#if NETFX_CORE
        private ProcessCpuUsage CurrentProcess;
#else
        private PerformanceCounter privilegedTimeCounter;
        private PerformanceCounter processorTimeCounter;
        private PerformanceCounter userTimeCounter;
#endif

        private bool monitorPrivilegedTime;
        private bool monitorProcessorTime;
        private bool monitorUserTime;

        private int monitorPeriodInMilliseconds;

        /// <summary>
        /// When used in .NET Core not supported monitoring of privileged time.
        /// </summary>
        /// <param name="monitorPrivilegedTime">If truth be monitored the privileged time. Not supported for .Net core.</param>
        /// <param name="monitorProcessorTime">If truth be monitored the processor time.</param>
        /// <param name="monitorUserTime">If truth be monitored the user time.</param>
        /// <param name="monitorPeriodInMilliseconds">The time interval between invocations of refresh, in milliseconds. Specify Timeout.Infinite to disable periodic signaling.</param>
        public CPUMonitor(bool monitorPrivilegedTime, bool monitorProcessorTime, bool monitorUserTime, int monitorPeriodInMilliseconds = 500)
        {
            if (!monitorPrivilegedTime && !monitorProcessorTime && !monitorUserTime)
                throw new ArgumentException("At least one flag has to be true.");

            this.monitorPrivilegedTime = monitorPrivilegedTime;
            this.monitorProcessorTime = monitorProcessorTime;
            this.monitorUserTime = monitorUserTime;

            this.monitorPeriodInMilliseconds = monitorPeriodInMilliseconds;

#if NETFX_CORE
            if (monitorPrivilegedTime)
                throw new NotSupportedException("Not support in .Net Core.");

            CurrentProcess = ProcessDiagnosticInfo.GetForCurrentProcess().CpuUsage;
#else
            string processName = Process.GetCurrentProcess().ProcessName;

            if (monitorPrivilegedTime)
                privilegedTimeCounter = new PerformanceCounter("Process", "% Privileged Time", processName);
            if (monitorProcessorTime)
                processorTimeCounter = new PerformanceCounter("Process", "% Processor Time", processName);
            if (monitorUserTime)
                userTimeCounter = new PerformanceCounter("Process", "% User Time", processName);
#endif

            timer = new Timer(DoMonitor, null, Timeout.Infinite, MonitorPeriodInMilliseconds);
        }

        public CPUMonitor(int monitorPeriodInMilliseconds = 1000)
            : this(true, true, true, monitorPeriodInMilliseconds)
        {
        }

        private void DoMonitor(object state)
        {
#if NETFX_CORE
            ProcessCpuUsageReport report = CurrentProcess.GetReport();

            if (monitorProcessorTime)
            {
                ProcessorTime = report.KernelTime.Ticks / System.Environment.ProcessorCount;

                if (ProcessorTime > PeakProcessorTime)
                    PeakProcessorTime = ProcessorTime;
            }

            if (monitorUserTime)
            {
                UserTime = report.UserTime.Ticks / System.Environment.ProcessorCount;

                if (UserTime > PeakUserTime)
                    PeakUserTime = UserTime;
            }
#else
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
#endif
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
#if !NETFX_CORE
            PrivilegedTime = 0;
            PeakPrivilegedTime = 0;
#endif

            ProcessorTime = 0;
            PeakProcessorTime = 0;

            UserTime = 0;
            PeakUserTime = 0;
        }

        /// <summary>
        /// Gets or sets the interval between every measure.
        /// </summary>
        private int MonitorPeriodInMilliseconds
        {
            get { return monitorPeriodInMilliseconds; }
            set
            {
                monitorPeriodInMilliseconds = value;
                timer.Change(0, monitorPeriodInMilliseconds);
            }
        }

#if !NETFX_CORE

        /// <summary>
        /// Shows the percentage of non-idle processor time spent executing code in privileged mode. Privileged mode is a processing mode designed for 
        /// operating system components and hardware-manipulating drivers. 
        /// It allows direct access to hardware and memory. The alternative, user mode, is a restricted processing mode designed for applications, 
        /// environmental subsystems, and integral subsystems. The operating system switches application threads to privileged mode to access operating system services.
        /// </summary>
        public float PrivilegedTime { get; private set; }

        /// <summary>
        /// Gets the peak privileged time value.
        /// </summary>
        public float PeakPrivilegedTime { get; private set; }
#endif

        /// <summary>
        /// Shows the percentage of time that the processor spent executing a non-idle thread. 
        /// It is calculated by measuring the duration that the idle thread is active during the sample interval, 
        /// and subtracting that time from 100 %. (Each processor has an idle thread that consumes cycles when no other threads are ready to run.)
        /// </summary>
        public float ProcessorTime { get; private set; }

        /// <summary>
        /// Gets the peak processor time value.
        /// </summary>
        public float PeakProcessorTime { get; private set; }

        /// <summary>
        /// Shows the percentage of time that the processor spent executing code in user mode. 
        /// Applications, environment subsystems, and integral subsystems execute in user mode. 
        /// Code executing in user mode cannot damage the integrity of the Windows Executive, kernel, and/or device drivers.
        /// </summary>
        public float UserTime { get; private set; }

        /// <summary>
        /// Gets the peak user time value.
        /// </summary>
        public float PeakUserTime { get; private set; }
    }
}
