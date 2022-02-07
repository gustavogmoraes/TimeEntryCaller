using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace TimeEntryCaller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var httpClient = new HttpClient { BaseAddress = new Uri(@"http://localhost:5000/api/TimeTracking/") };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            
            var credentials = GetCredentials();
            httpClient.PostAsync("Login", JsonContent.Create(credentials)).Wait();

            //var entries = GetEntriesFromCsv();    
            //httpClient.PostAsync("EnterTime", JsonContent.Create(entries)).Wait();
            
            var startDate = new DateTime(2021, 12, 01);
            var endDate = new DateTime(2021, 12, 31);
            
            var check = CheckHours(httpClient, startDate, endDate);
            
            //httpClient.PostAsJsonAsync("SyncNexonia", new { startDate, endDate }).Wait();
        }

        private void Test()
        {
            var startDate = new DateTime(2021, 07, 01);
            var endDate = new DateTime(2021, 07, 30);
         
            // var enteredEntries = GetEnteredEntries(httpClient, startDate, endDate);
            // var lastEntryFromEachDay = enteredEntries.GroupBy(x => x.Date).Where(x => new [] {"08/07/2021", "09/07/2021", "12/07/2021", "13/07/2021", "21/07/2021"}.Contains(x.Key)).Select(x => x.LastOrDefault()).ToList();
            // lastEntryFromEachDay.ForEach(x =>
            // {
            //     x.Description = x.Description += ".";
            //     x.Hours = 1.00M;
            //     x.Project = "BrightInsight - " + x.Project;
            //     x.FocalPoint = "Ana Irene Tovar";
            // });
            
            //httpClient.PostAsync("EnterTime", JsonContent.Create(lastEntryFromEachDay)).Wait();
            //var checks = CheckHours(httpClient, startDate, endDate);
        }

        private static IList<Entry> GetEnteredEntries(HttpClient httpClient, DateTime startDate, DateTime endDate)
        {
            var result = httpClient.GetAsync($"GetEntries?startDate={startDate}&endDate={endDate}").Result;
            var stringContent = result.Content.ReadAsStringAsync().Result;
            
            var enteredEntries = JsonConvert.DeserializeObject<IList<Entry>>(stringContent);

            return enteredEntries;
        }

        private static IList<dynamic> CheckHours(HttpClient httpClient, DateTime startDate, DateTime endDate)
        {
            var enteredEntries = GetEnteredEntries(httpClient, startDate, endDate);
            var results = enteredEntries
                .GroupBy(x => x.Date)
                .Select(x => (dynamic) new { Date = x.Key, Sum = x.Sum(y => y.Hours) })
                .Where(x => x.Sum != 9.00M)
                .ToList();

            var dayCount = enteredEntries
                .GroupBy(x => x.Date)
                .Count();
            var hourCount = enteredEntries
                .Sum(x => x.Hours);

            var finalCheck = hourCount / 9 == dayCount;
            
            results.Add(finalCheck ? "OK" : "Something wrong");

            return results;
        }

        private static Credentials GetCredentials()
        {
            return new Credentials
            {
                User = "gustavo.moraes",
                Password = "Krp6cd768d@123"
            };
        }

        private static List<Entry> GetEntriesFromCsv()
        {
            var hoursLines = File.ReadAllLines(@"/Users/gustavogmoraes/Desktop/Hours1.csv");
            var items = hoursLines.Select(x => x.Split(',')).ToList();
            items.RemoveAt(0);

            var entries = items.Select(x => new Entry
            {
                Date = x[0].Trim(),
                Hours = Convert.ToDecimal(x[1].Trim()),
                Project = x[2].Trim(),
                TaskCategory = x[3].Trim(),
                TaskDescription = x[4].Trim(),
                Comments = x[5].Trim(),
                FocalPoint = x[6].Trim()
            }).ToList();
            return entries;
        }

        private static DataTable ExcelToDataTable(string filePath)
        {
            //var existingFile = new FileInfo(filePath);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var fileStream = File.OpenRead(filePath);
            using var package = new ExcelPackage(fileStream);
            
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            int colCount = worksheet.Dimension.End.Column;
            int rowCount = worksheet.Dimension.End.Row;

            var dataTable = new DataTable();
            for (int row = 1; row <= rowCount; row++)
            {
                var cells = new List<string>();
                for (int col = 1; col <= colCount; col++)
                {
                    cells.Add(worksheet.Cells[row, col].Value?.ToString().Trim());
                }

                dataTable.Rows.Add(cells);
            }

            return dataTable;
        }
        
        private static List<Entry> GetEntriesFromXlsx()
        {
            var dt = ExcelToDataTable(@"/Users/gustavogmoraes/Desktop/Hours2.xlsx");
            
            var entries = dt.Rows.OfType<DataRow>()
                .Skip(1)
                .Where(x => !x[3].ToString().EndsWith("#"))
                .Select(x => new Entry
            {
                Date = x[0].ToString()?.Trim(),
                Hours = Convert.ToDecimal(x[1].ToString()?.Trim()),
                Project = x[2].ToString()?.Trim(),
                TaskCategory = x[3].ToString()?.Trim(),
                TaskDescription = x[4].ToString()?.Trim(),
                Comments = x[5].ToString()?.Trim(),
                FocalPoint = x[6].ToString()?.Trim()
            }).ToList();
            
            return entries;
        }
    }
    
    public class Entry
    {
        public string Date { get; set; }
        
        public string Project { get; set; }
        
        public decimal Hours { get; set; }
        
        public string TaskCategory { get; set; }
        
        public string TaskDescription { get; set; }

        public string Comments { get; set; }
        
        public string FocalPoint { get; set; }
    }
    
    public class Credentials
    {
        public string User { get; set; }
        
        public string Password { get; set; }
        
    }
}