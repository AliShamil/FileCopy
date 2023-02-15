using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileCopy
{
    public partial class MainWindow : Window
    {

        private string sourceFilePath;
        private string targetFilePath;
        private Thread copyThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelectSource_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txtSource.Text = openFileDialog.FileName;
                sourceFilePath = openFileDialog.FileName;
            }
        }

        private void btnSelectTarget_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                txtTarget.Text = saveFileDialog.FileName;
                targetFilePath = saveFileDialog.FileName;
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || string.IsNullOrWhiteSpace(targetFilePath))
            {
                MessageBox.Show("Please select source and target files.");
                return;
            }

            copyThread = new Thread(() =>
            {
                try
                {
                    using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
                    {
                        using (var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                        {
                            var buffer = new byte[4096];
                            var totalBytes = sourceStream.Length;
                            var copiedBytes = 0L;

                            while (copiedBytes < totalBytes)
                            {
                                var bytesToCopy = Math.Min(buffer.Length, (int)(totalBytes - copiedBytes));

                                sourceStream.Read(buffer, 0, bytesToCopy);
                                targetStream.Write(buffer, 0, bytesToCopy);

                                copiedBytes += bytesToCopy;
                                var progress = (int)(copiedBytes * 100 / totalBytes);
                                Dispatcher.Invoke(() =>
                                {
                                    progressBar.Value = progress;
                                    lblStatus.Content = progress + "%";
                                });

                                Thread.Sleep(500);
                            }

                            Dispatcher.Invoke(() =>
                            {
                                progressBar.Value = 100;
                                lblStatus.Content = "Completed";
                            });
                        }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = 0;
                        lblStatus.Content = "Suspended";
                    });
                }
                catch (ThreadAbortException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = 0;
                        lblStatus.Content = "Aborted";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnCopy.IsEnabled = true;
                    });
                }
            });
            btnCopy.IsEnabled = false;
            copyThread.Start();
        }

        private void btnSuspend_Click(object sender, RoutedEventArgs e)
        {
            copyThread.Suspend();
        }

        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            copyThread.Resume();
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {
            copyThread.Abort();
        }
    }
}

