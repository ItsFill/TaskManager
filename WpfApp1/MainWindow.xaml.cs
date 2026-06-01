using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace WpfApp1;

public class ProcessModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string StartTime { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }

    public ProcessModel(int id, string name, string startTime, string isRunning, string priority)
    {
        Id = id;
        Name = name;
        StartTime = startTime;
        Priority = priority;
        Status = isRunning;
    }

    public ProcessModel()
    {}

}

public partial class MainWindow : Window
{
    private DispatcherTimer _timer;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(2);
        _timer.Tick += Timer_Tick;
        _timer.Start();
        
        BtnKillProcess.Click += BtnKillProcess_Click;
        BtnToggleTimer.Click += BtnToggleTimer_Click;
        BtnStartProcess.Click += BtnStartProcess_Click;
        IntervalComboBox.SelectionChanged += IntervalComboBox_SelectionChanged;
        
        
        RefreshProcesses();
    }
    
    private void Timer_Tick(object sender, EventArgs e)
    {
        RefreshProcesses();
    }
    
    public void RefreshProcesses(){
        List<ProcessModel> myProcesses = new List<ProcessModel>();
        Process[] processes = Process.GetProcesses();
    
        foreach (Process p in processes)
        {
            ProcessModel temp = new ProcessModel();
            temp.Id = p.Id;
            temp.Name = p.ProcessName;
            temp.Status = p.Responding ? "Running" : "Not responding";
            try
            {
                temp.Priority = p.PriorityClass.ToString();
            }
            catch (Exception e)
            {
                temp.Priority = "No Access";
            }
            try
            {
                temp.StartTime = p.StartTime.ToShortDateString();
            }
            catch (Exception e)
            {
                temp.StartTime = "No Access";
            }
            myProcesses.Add(temp);
        }
    
        ProcessGrid.ItemsSource = myProcesses;
    }

    public void IntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_timer == null) return;
        
        if (IntervalComboBox.SelectedIndex == 0)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
        }
        else if (IntervalComboBox.SelectedIndex == 1)
        {
            _timer.Interval = TimeSpan.FromSeconds(2);
        } else if (IntervalComboBox.SelectedIndex == 2)
        {
            _timer.Interval = TimeSpan.FromSeconds(5);
        }
    }

    public void BtnStartProcess_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.DefaultExt = ".exe";
        dlg.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";

        if (dlg.ShowDialog() == true)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(dlg.FileName){ UseShellExecute = true };
                Process.Start(startInfo);
            
                RefreshProcesses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not start process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    public void BtnToggleTimer_Click(object sender, RoutedEventArgs e)
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            BtnToggleTimer.Content = "Resume";
        }
        else
        {
            _timer.Start();
            BtnToggleTimer.Content = "Pause";
        }
    }

    public void BtnKillProcess_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessGrid.SelectedItem is ProcessModel selectedProcess)
        {
            try
            {
                Process systemProcess = Process.GetProcessById(selectedProcess.Id);
                systemProcess.Kill();

                MessageBox.Show($"Process {selectedProcess.Name} was terminated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                RefreshProcesses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not terminate process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show("Please, select a process from the list first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
