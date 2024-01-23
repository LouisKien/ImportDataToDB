﻿using ImportDataToDB.Entity;
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
            var uniqueComboBoxItems = new HashSet<string>(comboBoxItems);

            object lockObject = new object();

            // Read lines in parallel
            Parallel.ForEach(File.ReadLines(filePath), line =>
            {
                // Parse fields from the line
                var fields = line.Split(',');

                // Check and add the unique item to comboBoxItems
                if (fields.Length > 6 && !uniqueComboBoxItems.Contains(fields[6]) && fields[6] != "Year")
                {
                    lock (lockObject)
                    {
                        comboBoxItems.Add(fields[6]);
                        uniqueComboBoxItems.Add(fields[6]);
                    }
                }

                // Add the fields to csvData
                lock (lockObject)
                {
                    csvData.Add(fields);
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
                            double rowScore = 0;
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
    }
}