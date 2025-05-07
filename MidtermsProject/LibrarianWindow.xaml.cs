using System.Windows;

namespace MidtermsProject
{
    public partial class LibrarianWindow : Window
    {
        public LibrarianWindow()
        {
            InitializeComponent();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}