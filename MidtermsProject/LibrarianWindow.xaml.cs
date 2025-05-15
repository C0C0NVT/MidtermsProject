using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MidtermsProject
{
    public partial class LibrarianWindow : Window
    {
        private users_table _currentUser;
        private bool _notificationVisible = false;

        public LibrarianWindow(users_table currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadData();
            UpdateWelcomeMessage();
            ShowBooks();
        }

        private void UpdateWelcomeMessage()
        {
            WelcomeTextBlock.Text = $"Welcome, {_currentUser.UserName}";
        }

        private void LoadData()
        {
            try
            {
                using (var db = new LibraryDataContextDataContext())
                {
                    BooksDataGrid.ItemsSource = db.Books.ToList();
                    TransactionsDataGrid.ItemsSource = db.BorrowTransactions.ToList();
                    StudentsDataGrid.ItemsSource = db.Students.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowBooks()
        {
            BooksTab.Visibility = Visibility.Visible;
            TransactionsTab.Visibility = Visibility.Collapsed;
            StudentsTab.Visibility = Visibility.Collapsed;
            BooksTab.IsSelected = true;

        }

        private void ShowTransactions()
        {
            BooksTab.Visibility = Visibility.Collapsed;
            TransactionsTab.Visibility = Visibility.Visible;
            StudentsTab.Visibility = Visibility.Collapsed;
            TransactionsTab.IsSelected = true;

        }

        private void ShowStudents()
        {
            BooksTab.Visibility = Visibility.Collapsed;
            TransactionsTab.Visibility = Visibility.Collapsed;
            StudentsTab.Visibility = Visibility.Visible;
            StudentsTab.IsSelected = true;

        }

        private void BooksButton_Click(object sender, RoutedEventArgs e) => ShowBooks();
        private void TransactionsButton_Click(object sender, RoutedEventArgs e) => ShowTransactions();
        private void StudentRecordsButton_Click(object sender, RoutedEventArgs e) => ShowStudents();

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addBookWindow = new AddEditBookWindow();
            if (addBookWindow.ShowDialog() == true)
            {
                LoadData(); // Refresh data after adding
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
                    LoadData(); // Refresh data after editing
                }
            }
            else
            {
                MessageBox.Show("Please select a book to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem != null)
            {
                var selectedBook = (Book)BooksDataGrid.SelectedItem;
                var result = MessageBox.Show($"Are you sure you want to delete '{selectedBook.Title}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new LibraryDataContextDataContext())
                        {
                            var bookToDelete = db.Books.FirstOrDefault(b => b.BookID == selectedBook.BookID);
                            if (bookToDelete != null)
                            {
                                db.Books.DeleteOnSubmit(bookToDelete);
                                db.SubmitChanges();
                                LoadData();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a book to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem != null)
            {
                var selectedTransaction = (BorrowTransaction)TransactionsDataGrid.SelectedItem;
                var updateWindow = new UpdateTransactionWindow(selectedTransaction);
                if (updateWindow.ShowDialog() == true)
                {
                    LoadData(); // Refresh data after update
                }
            }
            else
            {
                MessageBox.Show("Please select a transaction to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            _notificationVisible = !_notificationVisible;
            NotificationPanel.Visibility = _notificationVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SendNotification_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NotificationTextBox.Text))
            {
                try
                {
                    using (var db = new LibraryDataContextDataContext())
                    {
                        var notification = new Notification
                        {
                            SenderID = _currentUser.UserID,
                            RecipientRole = "Admin",
                            Message = NotificationTextBox.Text,
                            Timestamp = DateTime.Now,
                            IsRead = false
                        };
                        db.Notifications.InsertOnSubmit(notification);
                        db.SubmitChanges();
                        MessageBox.Show("Notification sent to admin!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        NotificationTextBox.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to send notification: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}