using ImportDataToDB.Data;
using ImportDataToDB.Entity;
using ImportDataToDB.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.Data;
using System.Diagnostics;
using System.IO;
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
using System.Linq;


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
        private List<AnalyseData> datas = new List<AnalyseData>();

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
            var uniqueComboBoxItems = new HashSet<string>(comboBoxItems); // Ensure uniqueness

            object lockObject = new object();

            // Read all lines
            var allLines = File.ReadAllLines(filePath);

            // Handle header separately
            if (allLines.Length > 0)
            {
                var header = allLines[0].Split(',');
                csvData.Add(header);
            }

            // Read remaining lines in parallel
            Parallel.ForEach(allLines.Skip(1), line =>
            {
                // Parse fields from the line
                var fields = line.Split(',');

                // Use a lock to ensure thread safety when modifying collections
                lock (lockObject)
                {
                    // Add the fields to csvData
                    csvData.Add(fields);

                    // Add the unique item to comboBoxItems
                    if (fields.Length > 6 && !uniqueComboBoxItems.Contains(fields[6]) && fields[6] != "Year")
                    {
                        uniqueComboBoxItems.Add(fields[6]);
                        comboBoxItems.Add(fields[6]);
                    }
                }
            });
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
            MessageBox.Show(importedItems[0][1].ToString());
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            AddDataToDatabase(importedItems);
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            MessageBox.Show($"Insert time: {elapsedTime}");
        }

        private void AddDataToDatabase(List<string[]> importedItems)
        {
            // Improve performance using Z.EntityFramework.Extensions.EF
            using (var context = new MyDbContext())
            {
                // Disable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                // Check if the SchoolYear with the given name already exists
                string schoolYearName = importedItems[1][6];
                if (context.SchoolYears.Any(sy => sy.Name == schoolYearName))
                {
                    MessageBox.Show($"Data in '{schoolYearName}' already exists, you can clear and import again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a list of subjects
                List<Subject> subjects = new List<Subject>
                {
                    new Subject { Code = "Math", Name = csvData[0][1] },
                    new Subject { Code = "Literature", Name = csvData[0][2] },
                    new Subject { Code = "Physics", Name = csvData[0][3] },
                    new Subject { Code = "Biology", Name = csvData[0][4] },
                    new Subject { Code = "ForeignLanguage", Name = csvData[0][5] },
                    new Subject { Code = "Chemistry", Name = csvData[0][7] },
                    new Subject { Code = "History", Name = csvData[0][8] },
                    new Subject { Code = "Geography", Name = csvData[0][9] },
                    new Subject { Code = "CivicEducation", Name = csvData[0][10] },
                };

                // Check and add subjects
                foreach (var subject in subjects)
                {
                    if (!context.Subjects.Any(s => s.Code == subject.Code))
                    {
                        context.Subjects.Add(subject);
                    }
                }

                // Create a new school year
                var schoolYear = new SchoolYear
                {
                    Name = schoolYearName,
                    ExamYear = schoolYearName,
                    Status = true
                };

                context.SchoolYears.Add(schoolYear);

                // Save changes to commit the subjects and school year to the database
                context.SaveChanges();

                // Fetch all subjects from the database
                List<Subject> allSubjects = context.Subjects.ToList();

                var students = new List<Student>();
                var scores = new List<Score>();

                foreach (var row in importedItems)
                {
                    var student = new Student
                    {
                        StudentCode = row[0],
                        SchoolYear = schoolYear
                    };

                    students.Add(student);

                    for (int i = 1; i <= 10; i++)
                    {
                        if (i != 6)
                        {
                            double rowScore = -1;
                            if (!row[i].Equals(""))
                            {
                                rowScore = double.Parse(row[i]);
                            }

                            // Assuming subjects is a List<Subject> to store the fetched subjects
                            Subject subjectToAdd = i > 6 ? allSubjects[i - 2] : allSubjects[i - 1];

                            var score = new Score
                            {
                                Result = rowScore,
                                Subject = subjectToAdd,
                                Student = student
                            };

                            scores.Add(score);
                        }
                    }
                }

                // Bulk insert students
                context.BulkInsert(students);

                // Bulk insert scores
                context.BulkInsert(scores);

                // Enable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = true;

                MessageBox.Show($"Data of {schoolYearName} saved in database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (txtPath.Text.Equals(""))
            {
                MessageBox.Show("Please select .csv file to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

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
                    // Increase command timeout
                    context.Database.SetCommandTimeout(300); // Set a timeout in seconds (e.g., 300 seconds = 5 minutes)

                    var studentIdsToDelete = context.Students
                        .Where(s => s.SchoolYearId == schoolYearToDelete.Id)
                        .Select(student => student.Id)
                        .ToList();

                    const int batchSize = 1000;
                    var batches = (int)Math.Ceiling((double)studentIdsToDelete.Count / batchSize);

                    for (int i = 0; i < batches; i++)
                    {
                        var batchIds = studentIdsToDelete.Skip(i * batchSize).Take(batchSize).ToList();

                        // Delete scores in batch
                        context.Scores.Where(s => batchIds.Contains(s.StudentId)).DeleteFromQuery();

                        // Delete students in batch
                        context.Students.Where(s => batchIds.Contains(s.Id)).DeleteFromQuery();
                    }

                    // Delete the SchoolYear
                    context.SchoolYears.Remove(schoolYearToDelete);

                    // Save changes to commit the deletions to the database
                    context.SaveChanges();
                }
            }

            MessageBox.Show($"All data of {cbYear.Text} has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            MessageBox.Show($"Delete time: {elapsedTime}");
        }

        private void ComboBoxSchoolYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected SchoolYear as a string from the ComboBox
            selectedYear = cbYear.SelectedItem as string;
        }

        private void btnAnalyse_Click(object sender, RoutedEventArgs e)
        {
            datas.Clear();
            lvAnalytic.ItemsSource = null;
            using (var context = new MyDbContext())
            {
                List<SchoolYear> schoolYears = context.SchoolYears.ToList();
                List<Subject> subjects = context.Subjects.ToList();
                List<int> yearId = new List<int>();
                foreach (var schoolYear in schoolYears)
                {
                    yearId.Add(schoolYear.Id);
                }

                foreach (var id in yearId)
                {
                    var schoolYear = context.SchoolYears.FirstOrDefault(s => s.Id == id);
                    long studentCount = context.Students.Count(student => student.SchoolYear.Id == id);
                    long mathCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 1 && s.Result >= 0);
                    long literatureCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 2 && s.Result >= 0);
                    long physicsCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 3 && s.Result >= 0);
                    long biologyCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 4 && s.Result >= 0);
                    long englishCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 5 && s.Result >= 0);
                    long chemistryCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 6 && s.Result >= 0);
                    long historyCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 7 && s.Result >= 0);
                    long geographyCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 8 && s.Result >= 0);
                    long civicEducationCount = context.Scores.Count(s => s.Student.SchoolYearId == id && s.SubjectId == 9 && s.Result >= 0);

                    datas.Add(new AnalyseData(schoolYear.Name, studentCount, mathCount, literatureCount, physicsCount, biologyCount, englishCount, chemistryCount, historyCount, geographyCount, civicEducationCount));
                }
            }
            lvAnalytic.ItemsSource = datas;
        }

        private void btnImportAll_Click(object sender, RoutedEventArgs e)
        {
            if (txtPath.Text.Equals(""))
            {
                MessageBox.Show("Please select .csv file to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            importedItems.Clear();
            AddAllDataToDatabase(csvData);
        }

        private async Task AddAllDataToDatabase(List<string[]> importedItems)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            using (var context = new MyDbContext())
            {
                // Disable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                // Fetch all subjects from the database
                List<Subject> allSubjects = context.Subjects.ToList();

                // Fetch all SchoolYear from the database
                List<SchoolYear> allSchoolYears = context.SchoolYears.ToList();

                // Check for existing school years
                foreach (string year in comboBoxItems)
                {
                    if (context.SchoolYears.Any(sy => sy.Name == year))
                    {
                        MessageBox.Show($"Data in '{year}' already exists, you can clear and import again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Create a list of subjects
                List<Subject> subjects = new List<Subject>
                {
                    new Subject { Code = "Math", Name = csvData[0][1] },
                    new Subject { Code = "Literature", Name = csvData[0][2] },
                    new Subject { Code = "Physics", Name = csvData[0][3] },
                    new Subject { Code = "Biology", Name = csvData[0][4] },
                    new Subject { Code = "ForeignLanguage", Name = csvData[0][5] },
                    new Subject { Code = "Chemistry", Name = csvData[0][7] },
                    new Subject { Code = "History", Name = csvData[0][8] },
                    new Subject { Code = "Geography", Name = csvData[0][9] },
                    new Subject { Code = "CivicEducation", Name = csvData[0][10] },
                };


                // Check and add subjects
                foreach (var subject in subjects)
                {
                    if (!allSubjects.Any(s => s.Code == subject.Code))
                    {
                        context.Subjects.Add(subject);
                        allSubjects.Add(subject);
                    }
                }

                // Create a new school year
                foreach (string year in comboBoxItems)
                {
                    var schoolYear = new SchoolYear
                    {
                        Name = year,
                        ExamYear = year,
                        Status = true
                    };

                    context.SchoolYears.Add(schoolYear);
                    allSchoolYears.Add(schoolYear);
                }

                // Save changes to commit the subjects and school year to the database
                await context.SaveChangesAsync();

                var students = new List<Student>();
                var scores = new List<Score>();

                int batchSize = 50000; // Adjust the batch size based on your system's capacity

                // Process and insert data in batches
                for (int i = 1; i < importedItems.Count; i += batchSize)
                {
                    var batchItems = importedItems.Skip(i).Take(batchSize);

                    foreach (var row in batchItems)
                    {
                        foreach (var year in allSchoolYears)
                        {
                            if (year.Name.Equals(row[6]))
                            {
                                var student = new Student
                                {
                                    StudentCode = row[0],
                                    SchoolYear = year
                                };

                                students.Add(student);

                                for (int j = 1; j <= 10; j++)
                                {
                                    if (j != 6)
                                    {
                                        double rowScore = -1;
                                        if (!row[j].Equals(""))
                                        {
                                            rowScore = double.Parse(row[j]);
                                        }

                                        Subject subjectToAdd = j > 6 ? allSubjects[j - 2] : allSubjects[j - 1];

                                        var score = new Score
                                        {
                                            Result = rowScore,
                                            Subject = subjectToAdd,
                                            Student = student
                                        };

                                        scores.Add(score);
                                    }
                                }
                            }
                        }
                    }

                    // Bulk insert students
                    await context.BulkInsertAsync(students);

                    // Bulk insert scores
                    await context.BulkInsertAsync(scores);

                    // Clear the lists for the next batch
                    students.Clear();
                    scores.Clear();
                }

                // Enable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            MessageBox.Show($"All data saved in the database in {elapsedTime}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}