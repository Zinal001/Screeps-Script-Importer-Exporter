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
using System.Windows.Shapes;

namespace Demo
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public String Username { get; private set; }
        public String Password { get; private set; }

        public Boolean RememberUsername { get; private set; }

        public Boolean Result { get; private set; }

        public LoginWindow(String Username = "", Boolean rememberUsername = true)
        {
            InitializeComponent();

            this.Username = Username;
            txtUsername.Text = Username;

            this.RememberUsername = rememberUsername;
            cbRemember.IsChecked = rememberUsername;


            this.Password = null;
            this.Result = false;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text.Length < 3)
            {
                MessageBox.Show("You have to specify a username", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(passPassword.Password.Length < 3)
            {
                MessageBox.Show("You have to specify a password", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.RememberUsername = cbRemember.IsChecked.Value;

            this.Username = txtUsername.Text;
            this.Password = passPassword.Password;
            this.Result = true;
            this.DialogResult = true;
            this.Close();
        }
    }
}
