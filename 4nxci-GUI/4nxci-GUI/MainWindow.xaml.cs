using System;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;

namespace NXCI_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!is_4nxci_exists())
            {
                System.Windows.Application.Current.Shutdown();
            }

            initializeKeyset();
        }

        private void initializeKeyset()
        {
            if (File.Exists(".\\keys.dat"))
            {
                txt_keyset.Text = Path.GetFullPath(".\\keys.dat");
            }
            else if (File.Exists(".\\keys.txt"))
            {
                txt_keyset.Text = Path.GetFullPath(".\\keys.txt");
            }
            else if (File.Exists(".\\keys.ini"))
            {
                txt_keyset.Text = Path.GetFullPath(".\\keys.ini");
            }
            else if (File.Exists(".\\prod.keys"))
            {
                txt_keyset.Text = Path.GetFullPath(".\\prod.keys");
            }
        }

        private bool is_4nxci_exists()
        {
            if (!(File.Exists(".\\4nxci.exe")))
            {
                System.Windows.MessageBox.Show("4nxci.exe is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool check_general_options()
        {
            if (txt_outdir.Text == string.Empty)
            {
                System.Windows.MessageBox.Show("Output directory path is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (txt_keyset.Text == string.Empty)
            {
                System.Windows.MessageBox.Show("Keyset path is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (txt_xci.Text == string.Empty)
            {
                System.Windows.MessageBox.Show("XCI path is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        static object x = new object();
        void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null)
                return;
            lock (x)
            {
                Dispatcher.Invoke(() =>
                {
                    txt_log.AppendText(e.Data + Environment.NewLine);
                    txt_log.ScrollToEnd();
                });
            }
        }

        private void launch_4nxci(string args)
        {
            if (is_4nxci_exists() == true && check_general_options() == true)
            {
                btn_convert.IsEnabled = false;
                string nxci_args;
                nxci_args = "\"" + txt_xci.Text + "\" -k \"" + txt_keyset.Text + "\" -o \"" + txt_outdir.Text + "\" " + args;
                Process nxci = new Process();
                nxci.StartInfo.FileName = ".\\4nxci.exe";
                nxci.StartInfo.Arguments = nxci_args;
                nxci.StartInfo.UseShellExecute = false;
                nxci.StartInfo.RedirectStandardOutput = true;
                nxci.StartInfo.RedirectStandardError = true;
                nxci.OutputDataReceived += OnOutputDataReceived;
                nxci.ErrorDataReceived += OnOutputDataReceived;
                nxci.StartInfo.CreateNoWindow = true;
                nxci.EnableRaisingEvents = true;
                nxci.Exited += new EventHandler(has_4ncxi_exited);
                nxci.Start();
                txt_log.Text = "4nxci execution started. Please wait...\n\n";
                nxci.BeginOutputReadLine();
                nxci.BeginErrorReadLine();
            }
        }

        private void has_4ncxi_exited(object sender, System.EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                btn_convert.IsEnabled = true;
            });
        }

        private void browse_folder(ref System.Windows.Controls.TextBox txtbox)
        {
            FolderBrowserDialog browse_dialog = new FolderBrowserDialog();
            DialogResult dialog_result = browse_dialog.ShowDialog();
            if (dialog_result == System.Windows.Forms.DialogResult.OK)
                txtbox.Text = browse_dialog.SelectedPath;
        }

        private void browse_folder(ref System.Windows.Controls.TextBox txtbox, string selectedPath)
        {
            FolderBrowserDialog browse_dialog = new FolderBrowserDialog();
            if (selectedPath != string.Empty)
            {
                browse_dialog.RootFolder = Environment.SpecialFolder.Desktop;
                browse_dialog.SelectedPath = selectedPath;
            }
            DialogResult dialog_result = browse_dialog.ShowDialog();
            if (dialog_result == System.Windows.Forms.DialogResult.OK)
                txtbox.Text = browse_dialog.SelectedPath;
        }

        private void browse_file(ref System.Windows.Controls.TextBox txtbox)
        {
            OpenFileDialog nca_browse_dialog = new OpenFileDialog();
            DialogResult dialog_result = nca_browse_dialog.ShowDialog();
            if (dialog_result == System.Windows.Forms.DialogResult.OK)
            {
                txtbox.Text = nca_browse_dialog.FileName;
                if (txtbox.Name == "txt_xci" && txt_outdir.Text == string.Empty)
                {
                    txt_outdir.Text = Path.GetDirectoryName(txt_xci.Text);
                }
            }
        }

        private void btn_browse_outdir_Click(object sender, RoutedEventArgs e)
        {
            if (txt_outdir.Text == string.Empty)
            {
                browse_folder(ref txt_outdir);
            }
            else
            {                
                browse_folder(ref txt_outdir, txt_outdir.Text);
            }            
        }

        private void btn_browse_keyset_Click(object sender, RoutedEventArgs e)
        {
            browse_file(ref txt_keyset);
        }

        private void btn_convert_Click(object sender, RoutedEventArgs e)
        {
            txt_log.Text = string.Empty;
            string args = string.Empty;
            if (chk_rename.IsChecked == true)
                args += "-r ";
            if (chk_keepncaid.IsChecked == true)
                args += "--keepncaid ";
            if (chk_tempfolder.IsChecked == true)
                args += "-t \"" + txt_outdir.Text + "\\4nxci_extracted_xci\"";
            launch_4nxci(args);
        }

        private void btn_xci_Click(object sender, RoutedEventArgs e)
        {
            browse_file(ref txt_xci);
        }

        private void txt_xci_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;
        }

        private void txt_xci_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                txt_xci.Text = files[0];
                if (txt_outdir.Text == string.Empty)
                {
                    txt_outdir.Text = Path.GetDirectoryName(txt_xci.Text);
                }
            }
        }

        private void txt_xci_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
