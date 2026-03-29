using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace KaliLinuxUI
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Operation> Operations { get; } = new();
        private readonly DispatcherTimer _timer;
        private readonly StringBuilder _terminalBuffer = new(4096);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SeedData();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += (_, _) => AppendTerminal($"[{Now()}] Packet capture...");
            _timer.Start();
        }

        private void SeedData()
        {
            Operations.Add(new() { Time = Now(), Tool = "nmap", Status = "Scanning..." });
            Operations.Add(new() { Time = Now(), Tool = "nikto", Status = "Checking..." });
        }

        private static string Now() => DateTime.Now.ToString("HH:mm:ss");

        // 🔥 Evita concatenación directa (mejor perf)
        private void AppendTerminal(string text)
        {
            _terminalBuffer.AppendLine(text);

            if (_terminalBuffer.Length > 10000)
                _terminalBuffer.Remove(0, 2000); // evita memory growth

            TerminalOutput.Text = _terminalBuffer.ToString();
            TerminalOutput.ScrollToEnd();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized 
                    ? WindowState.Normal 
                    : WindowState.Maximized;
            else
                DragMove();
        }

        private async void Tool_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            var tool = btn.Content.ToString()?.Replace("> ", "");

            Operations.Insert(0, new Operation
            {
                Time = Now(),
                Tool = tool,
                Status = "Starting..."
            });

            await RunToolAsync(tool);
        }

        private async Task RunToolAsync(string tool)
        {
            AppendTerminal($"root@kali# {tool}");
            await Task.Delay(400);
            AppendTerminal($"[+] Starting {tool}");
            await Task.Delay(600);
            AppendTerminal("[+] Done");
        }
    }

    public class Operation
    {
        public string Time { get; set; }
        public string Tool { get; set; }
        public string Status { get; set; }
    }
}
