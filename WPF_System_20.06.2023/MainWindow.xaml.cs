using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;

namespace WPF_System_20._06._2023
{
    public partial class MainWindow : Window
    {
        private List<ProcessItem> processList;
        private List<string> blacklistedProcesses;


        public MainWindow()
        {
            InitializeComponent();

            RefreshProcesses();

            blacklistedProcesses = new List<string>();
        }

        private void RefreshProcesses()
        {
            processList = Process.GetProcesses()
                .Select(p => new ProcessItem { Name = p.ProcessName, Id = p.Id, User = GetProcessOwner(p) })
                .ToList();

            processListView.ItemsSource = processList;
            detailsListView.ItemsSource = processList;
        }

        private string GetProcessOwner(Process process)
        {
            string processOwner = string.Empty;
            try
            {
                string query = "SELECT * FROM Win32_Process WHERE ProcessId = " + process.Id;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();

                foreach (ManagementObject obj in processList)
                {
                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                    {
                        processOwner = argList[1] + "\\" + argList[0]; // Domain\Username
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retrieve process owner: {ex.Message}");
            }

            return processOwner;
        }

        private void RunTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string taskPath = taskPathTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(taskPath))
            {
                try
                {
                    Process.Start(taskPath);
                    RefreshProcesses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to run task: {ex.Message}");
                }
            }
        }

        private void KillTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessItem selectedProcess = processListView.SelectedItem as ProcessItem;
            if (selectedProcess != null)
            {
                try
                {
                    Process.GetProcessById(selectedProcess.Id).Kill();
                    RefreshProcesses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to kill task: {ex.Message}");
                }
            }
        }

        private void BlacklistButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessItem selectedProcess = processListView.SelectedItem as ProcessItem;
            if (selectedProcess != null)
            {
                string processName = selectedProcess.Name;
                if (!blacklistedProcesses.Contains(processName))
                {
                    blacklistedProcesses.Add(processName);
                    MessageBox.Show($"Process '{processName}' has been blacklisted.");
                }
                else
                {
                    MessageBox.Show($"Process '{processName}' is already blacklisted.");
                }
            }
        }

        private void ProcessSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = processSearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                List<ProcessItem> filteredProcesses = processList.Where(p => p.Name.Contains(searchQuery)).ToList();
                processListView.ItemsSource = filteredProcesses;
            }
            else
            {
                processListView.ItemsSource = processList;
            }
        }

        private void DetailsSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = detailsSearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                List<ProcessItem> filteredProcesses = processList.Where(p => p.Name.Contains(searchQuery)).ToList();
                detailsListView.ItemsSource = filteredProcesses;
            }
            else
            {
                detailsListView.ItemsSource = processList;
            }
        }
    }

    public class ProcessItem
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string User { get; set; }
        public double CpuUsage { get; set; }
        public double GpuUsage { get; set; }
    }
}
