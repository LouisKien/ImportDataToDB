using ImportDataToDB.Entity;
using ImportDataToDB.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ImportDataToDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string[]> csvData = new List<string[]>();
        List<string> comboBoxItems = new List<string>();
        private List<string[]> importedItems = new List<string[]>();
        public MainWindow()
        {
            InitializeComponent();
            using (var context = new MyDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.Migrate();
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                txtPath.Text = openFileDialog.FileName;

                // Clear existing data before loading a new file
                csvData.Clear();
                comboBoxItems.Clear();
                cbYear.Items.Clear();
                cbYear.Text = "";

                // Read data from the CSV file
                csvData = ReadCsv(openFileDialog.FileName);

                cbYear.ItemsSource = comboBoxItems;
                cbYear.Text = comboBoxItems[0];
            }
        }

        private List<string[]> ReadCsv(string filePath)
        {
            var csvData = new List<string[]>();

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (!comboBoxItems.Contains(fields[6]) && !fields[6].Equals("Year"))
                    {
                        comboBoxItems.Add(fields[6]);
                    }
                    csvData.Add(fields);
                }
            }
            return csvData;
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if(txtPath.Text.Equals(""))
            {
                MessageBox.Show("Please select .csv file to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            importedItems.Clear();
            for(int i = 0; i < csvData.Count; i++)
            {
                importedItems.Add(csvData[i]);
            }
            AddDataToDatabase(importedItems);
        }

        private void AddDataToDatabase(List<string[]> importedItems)
        {
            using (var context = new MyDbContext())
            {
                // Check if the SchoolYear with the given name already exists
                List<SchoolYear> existingSchoolYears = context.SchoolYears.ToList();
                string schoolYearName = importedItems[1][6];
                SchoolYear schoolYearToAdd = existingSchoolYears.FirstOrDefault(sy => sy.Name == schoolYearName);
                if (schoolYearToAdd != null)
                {
                    MessageBox.Show($"Data in '{schoolYearName}' already exists, you can clear and import again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a list of subjects
                List<Subject> subjects = new List<Subject>
                {
                    new Subject { Code = "Math", Name = importedItems[0][1] },
                    new Subject { Code = "Literature", Name = importedItems[0][2] },
                    new Subject { Code = "Physics", Name = importedItems[0][3] },
                    new Subject { Code = "Biology", Name = importedItems[0][4] },
                    new Subject { Code = "ForeignLanguage", Name = importedItems[0][5] },
                    new Subject { Code = "Chemistry", Name = importedItems[0][7] },
                    new Subject { Code = "History", Name = importedItems[0][8] },
                    new Subject { Code = "Geography", Name = importedItems[0][9] },
                    new Subject { Code = "CivicEducation", Name = importedItems[0][10] },
                };

                foreach (var subject in subjects)
                {
                    // Check if the subject with the given code already exists
                    var existingSubject = context.Subjects.FirstOrDefault(s => s.Code == subject.Code);

                    // If the subject doesn't exist, add it to the Subject table
                    if (existingSubject == null)
                    {
                        context.Subjects.Add(subject);
                    }
                }

                var schoolYear = new SchoolYear
                {
                    Name = importedItems[1][6],
                    ExamYear = importedItems[1][6],
                    Status = bool.Parse("True")
                };

                context.SchoolYears.Add(schoolYear);

                // Save changes to commit the subjects to the database
                context.SaveChanges();

                // Skip the header row and iterate over the student rows
                foreach (var row in csvData.Skip(1))
                {
                    var student = new Student
                    {
                        StudentCode = row[0],
                        SchoolYear = schoolYear
                    };

                    context.Students.Add(student);

                    for (int i = 1; i <= 10; i++)
                    {
                        if (i != 6)
                        {
                            double rowScore = 0;
                            if (!row[i].Equals(""))
                            {
                                rowScore = double.Parse(row[i]);
                            }
                            if (i > 6)
                            {
                                var score = new Score
                                {
                                    Result = rowScore,
                                    Student = student,
                                    Subject = subjects[i - 2]
                                };

                                context.Scores.Add(score);
                            }
                            else
                            {
                                var score = new Score
                                {
                                    Result = rowScore,
                                    Student = student,
                                    Subject = subjects[i - 1]
                                };

                                context.Scores.Add(score);
                            }
                        }
                    }
                    context.SaveChanges();
                }
            }
            MessageBox.Show("Data saved in database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // Assuming your ComboBox is named comboBoxYear
            var selectedYear = cbYear.Text;

            if (string.IsNullOrEmpty(selectedYear))
            {
                MessageBox.Show("Please select year to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new MyDbContext())
            {
                // Find the SchoolYear by its name
                var schoolYearToDelete = context.SchoolYears.FirstOrDefault(y => y.Name == selectedYear);

                if (schoolYearToDelete != null)
                {
                    // Find all students associated with the SchoolYear
                    var studentsToDelete = context.Students.Where(s => s.SchoolYearId == schoolYearToDelete.Id).ToList();

                    // Get the StudentId values from studentsToDelete
                    var studentIdsToDelete = studentsToDelete.Select(student => student.Id).ToList();

                    // Find all scores associated with the students
                    var scoresToDelete = context.Scores
                        .Where(score => studentIdsToDelete.Contains(score.StudentId))
                        .ToList();

                    // Remove scores
                    context.Scores.RemoveRange(scoresToDelete);

                    // Remove students
                    context.Students.RemoveRange(studentsToDelete);

                    // Remove the SchoolYear
                    context.SchoolYears.Remove(schoolYearToDelete);

                    // Save changes to commit the deletions to the database
                    context.SaveChanges();
                }
            }
            MessageBox.Show($"All data of {cbYear.Text} has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}