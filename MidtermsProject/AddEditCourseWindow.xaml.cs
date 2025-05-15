using System;
using System.Linq;
using System.Windows;

namespace MidtermsProject
{
    public partial class AddEditCourseWindow : Window
    {
        private Course _editingCourse;

        public AddEditCourseWindow()
        {
            InitializeComponent();
        }

        public AddEditCourseWindow(Course courseToEdit) : this()
        {
            _editingCourse = courseToEdit;
            Title = "Edit Course";
            CourseIdTextBox.Text = courseToEdit.CourseID;
            CourseNameTextBox.Text = courseToEdit.CourseName;
            CourseIdTextBox.IsEnabled = false; // Don't allow editing ID
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CourseIdTextBox.Text) ||
                string.IsNullOrWhiteSpace(CourseNameTextBox.Text))
            {
                MessageBox.Show("Please fill all fields", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new LibraryDataContextDataContext())
                {
                    if (_editingCourse == null)
                    {
                        var newCourse = new Course
                        {
                            CourseID = CourseIdTextBox.Text,
                            CourseName = CourseNameTextBox.Text
                        };
                        db.Courses.InsertOnSubmit(newCourse);
                    }
                    else
                    {
                        var course = db.Courses.FirstOrDefault(c => c.CourseID == _editingCourse.CourseID);
                        if (course != null)
                        {
                            course.CourseName = CourseNameTextBox.Text;
                        }
                    }
                    db.SubmitChanges();
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving course: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}