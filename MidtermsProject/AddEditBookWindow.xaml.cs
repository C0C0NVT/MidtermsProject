using System;
using System.Linq;
using System.Windows;

namespace MidtermsProject
{
    public partial class AddEditBookWindow : Window
    {
        private Book _editingBook;

        public AddEditBookWindow()
        {
            InitializeComponent();
            LoadGenres();
        }

        public AddEditBookWindow(Book bookToEdit) : this()
        {
            _editingBook = bookToEdit;
            Title = "Edit Book";
            TitleTextBox.Text = bookToEdit.Title;
            AuthorTextBox.Text = bookToEdit.Author;
            YearTextBox.Text = bookToEdit.PublicationYear.ToString();
            CopiesTextBox.Text = bookToEdit.AvailableCopies.ToString();
            GenreComboBox.SelectedValue = bookToEdit.GenreID;
        }

        private void LoadGenres()
        {
            using (var db = new LibraryDataContextDataContext())
            {
                GenreComboBox.ItemsSource = db.Genres.ToList();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using (var db = new LibraryDataContextDataContext())
                {
                    if (_editingBook == null)
                    {
                        var newBook = new Book
                        {
                            BookID = "BOOK" + (db.Books.Count() + 1).ToString("D3"),
                            Title = TitleTextBox.Text,
                            Author = AuthorTextBox.Text,
                            PublicationYear = int.Parse(YearTextBox.Text),
                            AvailableCopies = int.Parse(CopiesTextBox.Text),
                            GenreID = (GenreComboBox.SelectedItem as Genre)?.GenreID
                        };
                        db.Books.InsertOnSubmit(newBook);
                    }
                    else
                    {
                        var book = db.Books.FirstOrDefault(b => b.BookID == _editingBook.BookID);
                        if (book != null)
                        {
                            book.Title = TitleTextBox.Text;
                            book.Author = AuthorTextBox.Text;
                            book.PublicationYear = int.Parse(YearTextBox.Text);
                            book.AvailableCopies = int.Parse(CopiesTextBox.Text);
                            book.GenreID = (GenreComboBox.SelectedItem as Genre)?.GenreID;
                        }
                    }
                    db.SubmitChanges();
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving book: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(AuthorTextBox.Text) ||
                string.IsNullOrWhiteSpace(YearTextBox.Text) ||
                string.IsNullOrWhiteSpace(CopiesTextBox.Text))
            {
                MessageBox.Show("Please fill all fields", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(YearTextBox.Text, out _) ||
                !int.TryParse(CopiesTextBox.Text, out _))
            {
                MessageBox.Show("Year and Copies must be numbers", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}