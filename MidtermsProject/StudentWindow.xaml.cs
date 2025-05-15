using System;
using System.Linq;
using System.Windows;

namespace MidtermsProject
{
    public partial class StudentWindow : Window
    {
        private users_table _currentUser;
        private bool _notificationVisible = false;

        public StudentWindow(users_table currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadData();
            UpdateWelcomeMessage();
            ShowProfile();
        }

        private void UpdateWelcomeMessage()
        {
            WelcomeTextBlock.Text = $"Hi, {_currentUser.UserName}! (ID: {_currentUser.StudentID})";
        }

        private void LoadData()
        {
            try
            {
                using (var db = new LibraryDataContextDataContext())
                {
                    CourseComboBox.ItemsSource = db.Courses.ToList();

                    // Load student profile
                    var student = db.Students.FirstOrDefault(s => s.StudentID == _currentUser.StudentID);
                    if (student != null)
                    {
                        StudentIDTextBlock.Text = student.StudentID;
                        NameTextBlock.Text = student.Name;
                        CourseComboBox.SelectedValue = student.CourseID;
                        AddressTextBox.Text = student.Address;
                        ContactTextBox.Text = student.ContactNumber;
                    }

                    // Load borrowed books
                    BorrowedBooksDataGrid.ItemsSource = db.BorrowTransactions
                        .Where(t => t.StudentID == _currentUser.StudentID && t.StatusID != "STAT2") // STAT2 = Returned
                        .Join(db.Books,
                            t => t.BookID,
                            b => b.BookID,
                            (t, b) => new {
                                b.Title,
                                b.Author,
                                t.BorrowDate,
                                DueDate = t.BorrowDate.HasValue ? t.BorrowDate.Value.AddDays(14) : (DateTime?)null,

                                Status = db.TransactionStatus.FirstOrDefault(s => s.StatusID == t.StatusID).TransactionStatus1
                            })
                        .ToList();

                    // Load available books
                    AvailableBooksDataGrid.ItemsSource = db.Books
                        .Where(b => b.AvailableCopies > 0)
                        .Select(b => new {
                            b.BookID,
                            b.Title,
                            b.Author,
                            b.PublicationYear,
                            b.AvailableCopies
                        })
                        .ToList();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new LibraryDataContextDataContext())
                {
                    var student = db.Students.FirstOrDefault(s => s.StudentID == _currentUser.StudentID);
                    if (student != null)
                    {
                        student.CourseID = (CourseComboBox.SelectedItem as Course)?.CourseID;
                        student.Address = AddressTextBox.Text;
                        student.ContactNumber = ContactTextBox.Text;
                        db.SubmitChanges();
                        MessageBox.Show("Profile updated successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving profile: {ex.Message}");
            }
        }
        private void ShowProfile()
        {
            ProfileTab.Visibility = Visibility.Visible;
            BorrowedBooksTab.Visibility = Visibility.Collapsed;
            CatalogTab.Visibility = Visibility.Collapsed;
            ProfileTab.IsSelected = true;

        }

        private void ShowBorrowedBooks()
        {
            ProfileTab.Visibility = Visibility.Collapsed;
            BorrowedBooksTab.Visibility = Visibility.Visible;
            CatalogTab.Visibility = Visibility.Collapsed;
            BorrowedBooksTab.IsSelected = true;

        }

        private void ShowCatalog()
        {
            ProfileTab.Visibility = Visibility.Collapsed;
            BorrowedBooksTab.Visibility = Visibility.Collapsed;
            CatalogTab.Visibility = Visibility.Visible;
            CatalogTab.IsSelected = true;

        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e) => ShowProfile();
        private void BorrowedBooksButton_Click(object sender, RoutedEventArgs e) => ShowBorrowedBooks();
        private void CatalogButton_Click(object sender, RoutedEventArgs e) => ShowCatalog();

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
                        MessageBox.Show("Your message has been sent to administrators.", "Notification Sent", MessageBoxButton.OK, MessageBoxImage.Information);
                        NotificationTextBox.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to send notification: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BorrowBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableBooksDataGrid.SelectedItem != null)
            {
                try
                {
                    var selectedItem = AvailableBooksDataGrid.SelectedItem;
                    var bookIdProperty = selectedItem.GetType().GetProperty("BookID");
                    var bookId = bookIdProperty.GetValue(selectedItem, null).ToString();

                    using (var db = new LibraryDataContextDataContext())
                    {
                        var transaction = new BorrowTransaction
                        {
                            TransactionID = "T" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            BookID = bookId,
                            StudentID = _currentUser.StudentID,
                            BorrowDate = DateTime.Now,
                            StatusID = "STAT1" // STAT1 = Borrowed
                        };

                        var book = db.Books.FirstOrDefault(b => b.BookID == bookId);
                        if (book != null)
                        {
                            book.AvailableCopies--;
                        }

                        db.BorrowTransactions.InsertOnSubmit(transaction);
                        db.SubmitChanges();
                        LoadData(); // Refresh the data after successful borrow
                        MessageBox.Show("Book borrowed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error borrowing book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a book to borrow.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ReturnBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BorrowedBooksDataGrid.SelectedItem != null)
            {
                try
                {
                    var selectedItem = BorrowedBooksDataGrid.SelectedItem;
                    var titleProperty = selectedItem.GetType().GetProperty("Title");
                    var title = titleProperty.GetValue(selectedItem, null).ToString();

                    using (var db = new LibraryDataContextDataContext())
                    {
                        var transaction = db.BorrowTransactions
                            .FirstOrDefault(t => t.StudentID == _currentUser.StudentID &&
                                              t.Book.Title == title &&
                                              t.StatusID == "STAT1"); // STAT1 = Borrowed

                        if (transaction != null)
                        {
                            transaction.ReturnDate = DateTime.Now;
                            transaction.StatusID = "STAT2"; // STAT2 = Returned

                            var book = db.Books.FirstOrDefault(b => b.BookID == transaction.BookID);
                            if (book != null)
                            {
                                book.AvailableCopies++;
                            }

                            db.SubmitChanges();
                            LoadData();
                            MessageBox.Show("Book returned successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error returning book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a book to return.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
        private void RefreshCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        private void CourseComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}