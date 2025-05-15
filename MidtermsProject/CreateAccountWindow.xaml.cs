using System;
using System.Linq;
using System.Windows;

namespace MidtermsProject
{
    public partial class CreateAccountWindow : Window
    {
        public CreateAccountWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string fullName = FullNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(fullName))
            {
                ShowError("Please fill all fields");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return;
            }

            using (var db = new LibraryDataContextDataContext())
            {
                if (db.users_tables.Any(u => u.UserID == username))
                {
                    ShowError("Username already exists");
                    return;
                }

                // Create new student record
                var newStudent = new Student
                {
                    StudentID = "S" + (db.Students.Count() + 1).ToString("D3"),
                    Name = fullName,
                    CourseID = "C03", // Default course
                    Address = "Not specified",
                    ContactNumber = "00000000000"
                };

                db.Students.InsertOnSubmit(newStudent);

                // Create new user account
                var newUser = new users_table
                {
                    UserID = username,
                    UserPass = MainWindow.HashPassword(password),
                    UserName = fullName,
                    UserRole = "Student",
                    StudentID = newStudent.StudentID
                };

                db.users_tables.InsertOnSubmit(newUser);

                try
                {
                    db.SubmitChanges();
                    MessageBox.Show("Account created successfully!\nYour Student ID: " + newStudent.StudentID,
                                  "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error creating account: " + ex.Message);
                }
            }
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}