using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace MidtermsProject
{
    public partial class MainWindow : Window
    {
        private readonly User[] _users = new[]
        {
            new User("Juan", HashPassword("password123"), "student"),
            new User("Angela", HashPassword("password456"), "librarian"),
            new User("Jill", HashPassword("password789"), "admin")
        };

        public MainWindow()
        {
            InitializeComponent();
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

            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ShowError("Invalid username or password");
                return;
            }

            switch (user.Role.ToLower())
            {
                case "student":
                    var studentWindow = new StudentWindow();
                    studentWindow.Show();
                    break;
                case "librarian":
                    var librarianWindow = new LibrarianWindow();
                    librarianWindow.Show();
                    break;
                case "admin":
                    var adminWindow = new AdminWindow();
                    adminWindow.Show();
                    break;
            }

            this.Close();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private static string HashPassword(string password)
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

    public class User
    {
        public string Username { get; }
        public string PasswordHash { get; }
        public string Role { get; }

        public User(string username, string passwordHash, string role)
        {
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
        }
    }
}