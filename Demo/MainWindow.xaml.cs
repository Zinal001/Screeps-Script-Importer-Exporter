using System;
using System.Collections.Generic;
using System.Linq;
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
using Ookii.Dialogs.Wpf;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using Zinal.Screeps.ScriptAPI;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private API ScriptAPI = null;

        public MainWindow()
        {
            InitializeComponent();

            txtPath.Text = Properties.Settings.Default.path;
            txtBranch.Text = Properties.Settings.Default.branch;

            ScriptAPI = new API(Properties.Settings.Default.path, Properties.Settings.Default.branch, Properties.Settings.Default.username);
        }

        private void txtPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.UseDescriptionForTitle = true;
            fbd.Description = "Select the folder to upload/download into";

            bool? res = fbd.ShowDialog(this);

            if (res.HasValue && res.Value)
                txtPath.Text = fbd.SelectedPath;

        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            if(ScriptAPI.Username == null || ScriptAPI.Password == null)
            {
                LoginWindow lw = new LoginWindow(Properties.Settings.Default.username);

                bool? dr = lw.ShowDialog();

                if (dr.HasValue && dr.Value && lw.Result)
                {
                    if (lw.RememberUsername)
                        Properties.Settings.Default.username = lw.Username;
                    ScriptAPI.SetAuthorization(lw.Username, lw.Password);
                }
                else
                    return;
            }

            pbWorking.Visibility = Visibility.Visible;
            ScriptAPI.ImportScripts(txtPath.Text, txtBranch.Text);

            if (ScriptAPI.LastError != null)
                MessageBox.Show(ScriptAPI.LastError.Message, "An error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show("All scripts imported");

            pbWorking.Visibility = Visibility.Hidden;

            Properties.Settings.Default.path = txtPath.Text;
            Properties.Settings.Default.branch = txtBranch.Text;
            Properties.Settings.Default.Save();
        }

        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptAPI.Username == null || ScriptAPI.Password == null)
            {
                LoginWindow lw = new LoginWindow(Properties.Settings.Default.username);

                bool? dr = lw.ShowDialog();

                if (dr.HasValue && dr.Value && lw.Result)
                {
                    if (lw.RememberUsername)
                        Properties.Settings.Default.username = lw.Username;
                    ScriptAPI.SetAuthorization(lw.Username, lw.Password);
                }
                else
                    return;
            }

            pbWorking.Visibility = Visibility.Visible;
            ScriptAPI.ExportScipts(txtPath.Text, txtBranch.Text);

            if (ScriptAPI.LastError != null)
                MessageBox.Show(ScriptAPI.LastError.Message, "An error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show("All scripts exported");
            pbWorking.Visibility = Visibility.Hidden;

            Properties.Settings.Default.path = txtPath.Text;
            Properties.Settings.Default.branch = txtBranch.Text;
            Properties.Settings.Default.Save();
        }


    }
}
