using System;
using System.Linq;
using System.Windows;

namespace MidtermsProject
{
    public partial class UpdateTransactionWindow : Window
    {
        private BorrowTransaction _transaction;

        public UpdateTransactionWindow(BorrowTransaction transaction)
        {
            InitializeComponent();
            _transaction = transaction;
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new LibraryDataContextDataContext())
            {
                StudentTextBlock.Text = _transaction.Student.Name;
                BookTextBlock.Text = _transaction.Book.Title;
                StatusComboBox.ItemsSource = db.TransactionStatus.ToList();
                StatusComboBox.SelectedValue = _transaction.StatusID;
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem == null) return;

            try
            {
                using (var db = new LibraryDataContextDataContext())
                {
                    var transaction = db.BorrowTransactions.FirstOrDefault(
                        t => t.TransactionID == _transaction.TransactionID);

                    if (transaction != null)
                    {
                        var newStatus = (TransactionStatus)StatusComboBox.SelectedItem;
                        transaction.StatusID = newStatus.StatusID;

                        // If marking as returned, update return date
                        if (newStatus.StatusID == "STAT2") // STAT2 = Returned
                        {
                            transaction.ReturnDate = DateTime.Now;
                            transaction.Book.AvailableCopies++;
                        }

                        db.SubmitChanges();
                        DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating transaction: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}