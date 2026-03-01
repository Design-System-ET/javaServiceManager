using System;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace JarServiceManager
{
    public partial class FormGraph : Form
    {
        private string serviceName;
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private Process proceso;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter memCounter;
        private const int MaxPoints = 60;

        public FormGraph(string serviceName)
        {
            InitializeComponent();
            this.serviceName = serviceName;
            this.Text = $"Service Performance - {serviceName}";
            InitChart();
            InitProcessCounters();

            timer.Interval = 500;
            timer.Tick += Timer_Tick;
            timer.Start();

            this.FormClosing += FormGraph_FormClosing;
        }

        private void InitChart()
        {
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.Legends.Clear();

            ChartArea area = new ChartArea("Area1");
            area.BackColor = Color.FromArgb(30, 30, 30);
            area.AxisX.LabelStyle.ForeColor = Color.White;
            area.AxisY.LabelStyle.ForeColor = Color.White;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisX.LabelStyle.Format = "HH:mm:ss";
            area.AxisX.TitleForeColor = Color.White;
            area.AxisY.TitleForeColor = Color.White;
            area.AxisX.Title = "Time";
            area.AxisY.Title = "CPU % / Memory MB";
            chart1.ChartAreas.Add(area);

            // CPU series
            Series cpuSeries = new Series("CPU");
            cpuSeries.ChartType = SeriesChartType.SplineArea;
            cpuSeries.Color = Color.FromArgb(120, 255, 0, 0);
            cpuSeries.BorderColor = Color.Red;
            cpuSeries.BorderWidth = 2;
            cpuSeries.XValueType = ChartValueType.DateTime;

            // Memory series
            Series memSeries = new Series("Memory");
            memSeries.ChartType = SeriesChartType.SplineArea;
            memSeries.Color = Color.FromArgb(120, 0, 0, 255);
            memSeries.BorderColor = Color.Blue;
            memSeries.BorderWidth = 2;
            memSeries.XValueType = ChartValueType.DateTime;

            chart1.Series.Add(cpuSeries);
            chart1.Series.Add(memSeries);

            Legend legend = new Legend("Legend1");
            legend.ForeColor = Color.White;
            chart1.Legends.Add(legend);

            chart1.BackColor = Color.FromArgb(30, 30, 30);
        }

        private void InitProcessCounters()
        {
            try
            {
                string jarName = serviceName.Substring(8) + ".jar";
                proceso = Process.GetProcessesByName("java")
                    .FirstOrDefault(p =>
                    {
                        try
                        {
                            using var searcher = new System.Management.ManagementObjectSearcher(
                                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {p.Id}");
                            var moc = searcher.Get().Cast<System.Management.ManagementBaseObject>().FirstOrDefault();
                            return moc?["CommandLine"]?.ToString().Contains(jarName, StringComparison.OrdinalIgnoreCase) ?? false;
                        }
                        catch { return false; }
                    });

                if (proceso != null)
                {
                    cpuCounter = new PerformanceCounter("Process", "% Processor Time", proceso.ProcessName, true);
                    memCounter = new PerformanceCounter("Process", "Working Set - Private", proceso.ProcessName, true);
                }
            }
            catch { }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (proceso == null || proceso.HasExited)
                return;

            float cpu = 0;
            float mem = 0;
            try
            {
                cpu = cpuCounter.NextValue() / Environment.ProcessorCount;
                mem = memCounter.NextValue() / 1024 / 1024;
            }
            catch { }

            DateTime now = DateTime.Now;

            var cpuSeries = chart1.Series["CPU"];
            var memSeries = chart1.Series["Memory"];

            cpuSeries.Points.AddXY(now, cpu);
            memSeries.Points.AddXY(now, mem);

            while (cpuSeries.Points.Count > MaxPoints)
                cpuSeries.Points.RemoveAt(0);
            while (memSeries.Points.Count > MaxPoints)
                memSeries.Points.RemoveAt(0);

            chart1.ChartAreas[0].AxisX.Minimum = DateTime.Now.AddSeconds(-MaxPoints * timer.Interval / 1000.0).ToOADate();
            chart1.ChartAreas[0].AxisX.Maximum = DateTime.Now.ToOADate();

            chart1.Invalidate();
        }

        private void FormGraph_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}