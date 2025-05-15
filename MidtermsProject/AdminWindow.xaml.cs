using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MidtermsProject
{
    public partial class AdminWindow : Window
    {
        private users_table _currentUser;
        private bool _notificationVisible = false;

        public AdminWindow(users_table currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadData();
            UpdateWelcomeMessage();
            ShowDashboard();
        }

        private void UpdateWelcomeMessage()
        {
            WelcomeTextBlock.Text = $"Welcome, {_currentUser.UserName} ({_currentUser.UserID})";
        }

        private void LoadData()
        {
            try
            {
                using (var db = new LibraryDataContextDataContext())
                {
                    UsersDataGrid.ItemsSource = db.users_tables.ToList();
                    StudentsDataGrid.ItemsSource = db.Students.ToList();
                    BooksDataGrid.ItemsSource = db.Books.ToList();
                    TransactionsDataGrid.ItemsSource = db.BorrowTransactions.ToList();
                    CoursesDataGrid.ItemsSource = db.Courses.ToList();
                    NotificationsListView.ItemsSource = db.Notifications
                        .OrderByDescending(n => n.Timestamp)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowDashboard()
        {
            DashboardTab.Visibility = Visibility.Visible;
            UserManagementTab.Visibility = Visibility.Collapsed;
            StudentRecordsTab.Visibility = Visibility.Collapsed;
            BookManagementTab.Visibility = Visibility.Collapsed;
            TransactionsTab.Visibility = Visibility.Collapsed;
            CourseManagementTab.Visibility = Visibility.Collapsed;
            DashboardTab.IsSelected = true;
        }

        private void ShowUserManagement()
        {
            DashboardTab.Visibility = Visibility.Collapsed;
            UserManagementTab.Visibility = Visibility.Visible;
            StudentRecordsTab.Visibility = Visibility.Collapsed;
            BookManagementTab.Visibility = Visibility.Collapsed;
            TransactionsTab.Visibility = Visibility.Collapsed;
            CourseManagementTab.Visibility = Visibility.Collapsed;
            UserManagementTab.IsSelected = true;
        }

        private void ShowStudentRecords()
        {
            DashboardTab.Visibility = Visibility.Collapsed;
            UserManagementTab.Visibility = Visibility.Collapsed;
            StudentRecordsTab.Visibility = Visibility.Visible;
            BookManagementTab.Visibility = Visibility.Collapsed;
            TransactionsTab.Visibility = Visibility.Collapsed;
            CourseManagementTab.Visibility = Visibility.Collapsed;
            StudentRecordsTab.IsSelected = true;
        }

        private void ShowBookManagement()
        {
            DashboardTab.Visibility = Visibility.Collapsed;
            UserManagementTab.Visibility = Visibility.Collapsed;
            StudentRecordsTab.Visibility = Visibility.Collapsed;
            BookManagementTab.Visibility = Visibility.Visible;
            TransactionsTab.Visibility = Visibility.Collapsed;
            CourseManagementTab.Visibility = Visibility.Collapsed;
            BookManagementTab.IsSelected = true;
        }

        private void ShowTransactions()
        {
            DashboardTab.Visibility = Visibility.Collapsed;
            UserManagementTab.Visibility = Visibility.Collapsed;
            StudentRecordsTab.Visibility = Visibility.Collapsed;
            BookManagementTab.Visibility = Visibility.Collapsed;
            TransactionsTab.Visibility = Visibility.Visible;
            CourseManagementTab.Visibility = Visibility.Collapsed;
            TransactionsTab.IsSelected = true;
        }

        private void ShowCourseManagement()
        {
            DashboardTab.Visibility = Visibility.Collapsed;
            UserManagementTab.Visibility = Visibility.Collapsed;
            StudentRecordsTab.Visibility = Visibility.Collapsed;
            BookManagementTab.Visibility = Visibility.Collapsed;
            TransactionsTab.Visibility = Visibility.Collapsed;
            CourseManagementTab.Visibility = Visibility.Visible;
            CourseManagementTab.IsSelected = true;
        }


        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is users_table selectedUser)
            {
                try
                {
                    using (var db = new LibraryDataContextDataContext())
                    {
                        var userToUpdate = db.users_tables.FirstOrDefault(u => u.UserID == selectedUser.UserID);
                        if (userToUpdate != null)
                        {
                            var comboBox = (ComboBox)sender;
                            userToUpdate.UserRole = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                            db.SubmitChanges();

                            // Refresh the data grid
                            UsersDataGrid.ItemsSource = db.users_tables.ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating role: {ex.Message}");
                }
            }
        }

        // Button click handlers
        private void DashboardButton_Click(object sender, RoutedEventArgs e) => ShowDashboard();
        private void UserManagementButton_Click(object sender, RoutedEventArgs e) => ShowUserManagement();
        private void StudentRecordsButton_Click(object sender, RoutedEventArgs e) => ShowStudentRecords();
        private void BookManagementButton_Click(object sender, RoutedEventArgs e) => ShowBookManagement();
        private void TransactionsButton_Click(object sender, RoutedEventArgs e) => ShowTransactions();
        private void CourseManagementButton_Click(object sender, RoutedEventArgs e) => ShowCourseManagement();

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addBookWindow = new AddEditBookWindow();
            if (addBookWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem != null)
            {
                var selectedBook = (Book)BooksDataGrid.SelectedItem;
                var editWindow = new AddEditBookWindow(selectedBook);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem != null)
            {
                var selectedBook = (Book)BooksDataGrid.SelectedItem;
                var result = MessageBox.Show($"Delete '{selectedBook.Title}'?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new LibraryDataContextDataContext())
                        {
                            var book = db.Books.FirstOrDefault(b => b.BookID == selectedBook.BookID);
                            if (book != null)
                            {
                                db.Books.DeleteOnSubmit(book);
                                db.SubmitChanges();
                                LoadData();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var addCourseWindow = new AddEditCourseWindow();
            if (addCourseWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem != null)
            {
                var selectedCourse = (Course)CoursesDataGrid.SelectedItem;
                var editWindow = new AddEditCourseWindow(selectedCourse);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void DeleteCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem != null)
            {
                var selectedCourse = (Course)CoursesDataGrid.SelectedItem;
                var result = MessageBox.Show($"Delete '{selectedCourse.CourseName}'?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new LibraryDataContextDataContext())
                        {
                            var course = db.Courses.FirstOrDefault(c => c.CourseID == selectedCourse.CourseID);
                            if (course != null)
                            {
                                db.Courses.DeleteOnSubmit(course);
                                db.SubmitChanges();
                                LoadData();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            _notificationVisible = !_notificationVisible;
            NotificationPanel.Visibility = _notificationVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MarkAsReadButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotificationsListView.SelectedItem != null)
            {
                var notification = (Notification)NotificationsListView.SelectedItem;
                try
                {
                    using (var db = new LibraryDataContextDataContext())
                    {
                        var dbNotification = db.Notifications.FirstOrDefault(n => n.NotificationID == notification.NotificationID);
                        if (dbNotification != null)
                        {
                            dbNotification.IsRead = true;
                            db.SubmitChanges();
                            LoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void RefreshCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is users_table selectedUser)
            {
                // Prevent deleting yourself
                if (selectedUser.UserID == _currentUser.UserID)
                {
                    MessageBox.Show("You cannot delete your own account!", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to delete user '{selectedUser.UserName}'?",
                                           "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new LibraryDataContextDataContext())
                        {
                            var userToDelete = db.users_tables.FirstOrDefault(u => u.UserID == selectedUser.UserID);
                            if (userToDelete != null)
                            {
                                // If it's a student, delete their student record first
                                if (!string.IsNullOrEmpty(userToDelete.StudentID))
                                {
                                    var student = db.Students.FirstOrDefault(s => s.StudentID == userToDelete.StudentID);
                                    if (student != null)
                                    {
                                        db.Students.DeleteOnSubmit(student);
                                    }
                                }

                                db.users_tables.DeleteOnSubmit(userToDelete);
                                db.SubmitChanges();
                                LoadData(); // Refresh the data grid

                                MessageBox.Show("User deleted successfully!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting user: {ex.Message}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

    }
}