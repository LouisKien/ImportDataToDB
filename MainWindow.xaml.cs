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
using System;


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
                context.Database.SetCommandTimeout(3600);
            }
            AddProvinceToDatabase();
        }

        private List<string[]> ReadProvinceCsv()
        {
            string filePath = System.IO.Path.Combine(System.IO.Path.GetFullPath(@"..\..\..\"), "Tinh.csv");

            var csvData = new List<string[]>();

            object lockObject = new object();

            // Read all lines
            var allLines = File.ReadAllLines(filePath);

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
                }
            });
            return csvData;
        }

        private void AddProvinceToDatabase()
        {
            List<string[]> provinceList = ReadProvinceCsv();

            using (var context = new MyDbContext())
            {
                // Disable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var provinces = new List<Province>();

                for (int i = 0; i < provinceList.Count; i++)
                {
                    var province = new Province
                    {
                        MaTinh = int.Parse(provinceList[i][0]),
                        TenTinh = provinceList[i][1]
                    };
                    provinces.Add(province);
                }
                context.BulkInsert(provinces);
                // Enable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = true;
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
                if (csvData[i][6].Equals(this.selectedYear) || csvData[i][6].Equals("Year"))
                {
                    importedItems.Add(csvData[i]);
                }
            }
            AddDataToDatabase(importedItems);
        }

        private async Task AddDataToDatabase(List<string[]> importedItems)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            using (var context = new MyDbContext())
            {
                // Disable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                // Fetch all subjects from the database asynchronously
                List<Subject> allSubjects = await context.Subjects.ToListAsync();

                // Fetch all SchoolYear from the database asynchronously
                List<SchoolYear> allSchoolYears = await context.SchoolYears.ToListAsync();

                List<Province> allProvinces = await context.Provinces.ToListAsync();

                // Check for existing school years
                if (context.SchoolYears.Any(sy => sy.Name == selectedYear))
                {
                    MessageBox.Show($"Data in '{selectedYear}' already exists, you can clear and import again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                // Check and add subjects asynchronously
                foreach (var subject in subjects)
                {
                    if (!allSubjects.Any(s => s.Code == subject.Code))
                    {
                        context.Subjects.Add(subject);
                        allSubjects.Add(subject);
                    }
                }

                // Create a new school year asynchronously
                var schoolYear = new SchoolYear
                {
                    Name = selectedYear,
                    ExamYear = selectedYear,
                    Status = true
                };

                context.SchoolYears.Add(schoolYear);
                allSchoolYears.Add(schoolYear);

                // Save changes to commit the subjects and school year to the database asynchronously
                await context.SaveChangesAsync();

                int batchSize = 100000; // Adjust the batch size based on your system's capacity

                // Process and insert data in batches asynchronously
                for (int i = 1; i < importedItems.Count; i += batchSize)
                {
                    var batchItems = importedItems.Skip(i).Take(batchSize).ToList();

                    var localStudents = new List<Student>();
                    var localScores = new List<Score>();

                    var tasks = batchItems.Select(async row =>
                    {
                        foreach (var year in allSchoolYears)
                        {
                            foreach(var province in allProvinces)
                            {
                                if (year.ExamYear.Equals(selectedYear) && year.Name.Equals(row[6]) && province.MaTinh == int.Parse(row[11]))
                                {
                                    var student = new Student
                                    {
                                        StudentCode = row[0],
                                        SchoolYear = year,
                                        Province = province
                                    };

                                    lock (localStudents)
                                    {
                                        localStudents.Add(student);
                                    }

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

                                            lock (localScores)
                                            {
                                                localScores.Add(score);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    // Wait for all tasks in the batch to complete
                    await Task.WhenAll(tasks);

                    // Bulk insert students asynchronously
                    await context.BulkInsertAsync(localStudents);

                    // Bulk insert scores asynchronously
                    await context.BulkInsertAsync(localScores);
                }

                // Enable AutoDetectChanges
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            MessageBox.Show($"{selectedYear} data saved in the database in {elapsedTime}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
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
                    MessageBox.Show($"All data of {cbYear.Text} has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    stopWatch.Stop();

                    // Get the elapsed time as a TimeSpan value.
                    TimeSpan ts = stopWatch.Elapsed;

                    // Format and display the TimeSpan value.
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    MessageBox.Show($"Delete time: {elapsedTime}");
                } else
                {
                    MessageBox.Show($"Data of {selectedYear} does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
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

                if (schoolYears.Count == 0)
                {
                    MessageBox.Show("No data to analyse.");
                }

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

                // Fetch all subjects from the database asynchronously
                List<Subject> allSubjects = await context.Subjects.ToListAsync();

                // Fetch all SchoolYear from the database asynchronously
                List<SchoolYear> allSchoolYears = await context.SchoolYears.ToListAsync();

                List<Province> allProvinces = await context.Provinces.ToListAsync();

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

                // Check and add subjects asynchronously
                foreach (var subject in subjects)
                {
                    if (!allSubjects.Any(s => s.Code == subject.Code))
                    {
                        context.Subjects.Add(subject);
                        allSubjects.Add(subject);
                    }
                }

                // Create a new school year asynchronously
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

                // Save changes to commit the subjects and school year to the database asynchronously
                await context.SaveChangesAsync();

                int batchSize = 100000000; // Adjust the batch size based on your system's capacity

                // Process and insert data in batches asynchronously
                for (int i = 1; i < importedItems.Count; i += batchSize)
                {
                    var batchItems = importedItems.Skip(i).Take(batchSize).ToList();

                    var localStudents = new List<Student>();
                    var localScores = new List<Score>();

                    var tasks = batchItems.Select(async row =>
                    {
                        foreach (var year in allSchoolYears)
                        {
                            foreach(var province in allProvinces)
                            {
                                if (year.Name.Equals(row[6]) && province.MaTinh == int.Parse(row[11]))
                                {
                                    var student = new Student
                                    {
                                        StudentCode = row[0],
                                        SchoolYear = year,
                                        Province = province
                                    };

                                    lock (localStudents)
                                    {
                                        localStudents.Add(student);
                                    }

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

                                            lock (localScores)
                                            {
                                                localScores.Add(score);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    // Wait for all tasks in the batch to complete
                    await Task.WhenAll(tasks);

                    // Bulk insert students asynchronously
                    await context.BulkInsertAsync(localStudents);

                    // Bulk insert scores asynchronously
                    await context.BulkInsertAsync(localScores);
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

        private void btnStats_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedYear))
            {
                MessageBox.Show("Please import data and select year first.");
                return;
            }
            using (var context = new MyDbContext()) { 
                List<SchoolYear> schoolYears = context.SchoolYears.ToList();
                if (context.SchoolYears.Where(y => selectedYear.Contains(y.ExamYear)).ToList().Count == 0)
                {
                    MessageBox.Show($"{selectedYear} has no data to view Valedictorian Statistics.");
                    return;
                }
            }
            Statistics statistics = new Statistics(selectedYear);
            statistics.Show();
        }
    }
}