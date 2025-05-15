using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace MidtermsProject
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UsernameTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password");
                return;
            }

            using (var db = new LibraryDataContextDataContext())
            {
                var user = db.users_tables.FirstOrDefault(u => u.UserID == username);

                if (user == null)
                {
                    ShowError("User does not exist");
                    return;
                }

                if (!VerifyPassword(password, user.UserPass))
                {
                    ShowError("Invalid password");
                    return;
                }

                switch (user.UserRole.ToLower())
                {
                    case "student":
                        var studentWindow = new StudentWindow(user);
                        studentWindow.Show();
                        break;
                    case "librarian":
                        var librarianWindow = new LibrarianWindow(user);
                        librarianWindow.Show();
                        break;
                    case "admin":
                        var adminWindow = new AdminWindow(user);
                        adminWindow.Show();
                        break;
                }

                this.Close();
            }
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var createAccountWindow = new CreateAccountWindow();
            createAccountWindow.ShowDialog();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private static bool VerifyPassword(string inputPassword, string storedHash)
        {
            var hashOfInput = HashPassword(inputPassword);
            return string.Equals(hashOfInput, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}