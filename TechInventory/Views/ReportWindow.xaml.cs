using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace TechInventory.Views
{
    public partial class ReportWindow : Window
    {
        private readonly string _dbPath;

        public ReportWindow(string dbPath)
        {
            InitializeComponent();
            _dbPath = dbPath;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string reportType;
            if (PurchaseRadio.IsChecked == true)
                reportType = "purchase";
            else if (InventoryRadio.IsChecked == true)
                reportType = "inventory";
            else
                reportType = "tickets";

            try
            {
                string scriptDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
                string scriptPath = Path.Combine(scriptDir, "report_generator.py");
                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show("Скрипт генерации отчётов не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                Directory.CreateDirectory(outputDir);

                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{scriptPath}\" {reportType} \"{_dbPath}\" \"{outputDir}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show($"Ошибка выполнения Python-скрипта:\n{error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show($"Отчёт успешно создан.\n{output}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (Directory.Exists(outputDir))
                        Process.Start("explorer.exe", outputDir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nУбедитесь, что Python установлен и доступен в PATH.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}