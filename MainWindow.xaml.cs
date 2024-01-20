using ImportDataToDB.Entity;
using ImportDataToDB.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.Data;
using System.Diagnostics;
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
        private string selectedYear;
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
                cbYear.ItemsSource = null;
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
                if (csvData[i][6].Equals(this.selectedYear))
                {
                    importedItems.Add(csvData[i]);
                }
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            AddDataToDatabase(importedItems);
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            MessageBox.Show($"RunTime + {elapsedTime}");
        }

        private void AddDataToDatabase(List<string[]> importedItems)
        {
            using (var context = new MyDbContext())
            {
                string schoolYearName = importedItems[1][6];

                // Check if the SchoolYear with the given name already exists
                if (context.SchoolYears.Any(sy => sy.Name == schoolYearName))
                {
                    MessageBox.Show($"Data in '{schoolYearName}' already exists, you can clear and import again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a DataTable for scores
                var dataTable = new System.Data.DataTable();
                dataTable.Columns.Add("Result", typeof(double));
                dataTable.Columns.Add("StudentId", typeof(int));
                dataTable.Columns.Add("SubjectId", typeof(int));

                // Fetch all subjects from the database
                Dictionary<string, int> subjectIdMap = context.Subjects.ToDictionary(s => s.Code, s => s.Id);

                var schoolYear = new SchoolYear
                {
                    Name = schoolYearName,
                    ExamYear = schoolYearName,
                    Status = true
                };

                context.SchoolYears.Add(schoolYear);
                context.SaveChanges();

                // Skip the header row and iterate over the student rows
                foreach (var row in importedItems.Skip(1))
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

                            if (subjectIdMap.TryGetValue(row[i], out int subjectId))
                            {
                                dataTable.Rows.Add(rowScore, student.Id, subjectId);
                            }
                        }
                    }

                    if (dataTable.Rows.Count >= 5000)
                    {
                        // Use SqlBulkCopy to insert data in bulk
                        using (var bulkCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString))
                        {
                            bulkCopy.BatchSize = 5000;
                            bulkCopy.DestinationTableName = "Scores";
                            bulkCopy.ColumnMappings.Add("Result", "Result");
                            bulkCopy.ColumnMappings.Add("StudentId", "StudentId");
                            bulkCopy.ColumnMappings.Add("SubjectId", "SubjectId");

                            bulkCopy.WriteToServer(dataTable);
                        }

                        dataTable.Rows.Clear();
                    }
                }

                if (dataTable.Rows.Count > 0)
                {
                    // Use SqlBulkCopy to insert the remaining data
                    using (var bulkCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString))
                    {
                        bulkCopy.BatchSize = 400;
                        bulkCopy.DestinationTableName = "Scores";
                        bulkCopy.ColumnMappings.Add("Result", "Result");
                        bulkCopy.ColumnMappings.Add("StudentId", "StudentId");
                        bulkCopy.ColumnMappings.Add("SubjectId", "SubjectId");

                        bulkCopy.WriteToServer(dataTable);
                    }
                }

                // Save changes to commit the students and scores to the database
                context.SaveChanges();
            }

            //MessageBox.Show($"Data of {this.selectedYear} saved in database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (txtPath.Text.Equals(""))
            {
                MessageBox.Show("Please select .csv file to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Assuming your ComboBox is named comboBoxYear
            var selectedYear = this.selectedYear;

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
        private void ComboBoxSchoolYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected SchoolYear as a string from the ComboBox
            selectedYear = cbYear.SelectedItem as string;
        }
    }
}