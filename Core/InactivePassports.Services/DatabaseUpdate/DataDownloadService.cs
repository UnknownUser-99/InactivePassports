using System.Text;
using System.Globalization;
using InactivePassports.Contracts.DatabaseUpdate;
using InactivePassports.Data.Configurations;
using InactivePassports.Data.Entities.File;
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using CsvHelper;
using CsvHelper.Configuration;

namespace InactivePassports.Services.DatabaseUpdate
{
    public class DataDownloadService : IDataDownload
    {
        private readonly DataDownloadOptions _options;

        public DataDownloadService(DataDownloadOptions options)
        {
            _options = options;
        }

        public void DownloadFile()
        {
            if (!File.Exists(_options.CredentialsPath))
            {
                throw new FileNotFoundException("Файл не найден.", _options.CredentialsPath);
            }

            string[] scopes = { DriveService.Scope.DriveReadonly };
            string ApplicationName = _options.ApplicationName;

            UserCredential credential;

            using (FileStream stream = new FileStream(_options.CredentialsPath, FileMode.Open, FileAccess.Read))
            {
                string credPath = _options.TokensPath;

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var request = service.Files.Get(_options.FileId);

            using (var memoryStream = new MemoryStream())
            {
                request.MediaDownloader.ProgressChanged += progress =>
                {
                    if (progress.Status == Google.Apis.Download.DownloadStatus.Failed)
                    {
                        throw new FileNotFoundException("Не удалось загрузить файл.");
                    }
                };

                request.Download(memoryStream);

                using (FileStream fileStream = new FileStream(_options.DataPath, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fileStream);
                }
            }
        }

        public HashSet<Passport> ReadPassports(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Файл не найден.", path);
            }

            int lineCount = LineCountCheck(path);

            HashSet<Passport> passports = new HashSet<Passport>(lineCount - 1);

            using (StreamReader streamReader = new StreamReader(path))
            {
                using (CsvReader csv = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    foreach (Passport passport in csv.GetRecords<Passport>())
                    {
                        passports.Add(passport);
                    }
                }
            }

            return passports;
        }

        public IEnumerable<List<Passport>> ReadFile()
        {
            string path = _options.DataPath;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Файл Data.csv не найден.", path);
            }

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                Encoding = Encoding.UTF8
            };

            int lineCount = LineCountCheck(path);
            int returnCount = 0;
            int sizeCollection = _options.SizeCollection;

            List<Passport> passports = new List<Passport>(sizeCollection);

            using (StreamReader streamReader = new StreamReader(path))
            {
                streamReader.ReadLine();

                using (CsvReader csv = new CsvReader(streamReader, csvConfig))
                {
                    while (csv.Read())
                    {
                        if (csv.TryGetField<int>(0, out var series) && csv.TryGetField<int>(1, out var number))
                        {
                            if (ValidationCheck(series, number))
                            {
                                passports.Add(new Passport
                                {
                                    Series = series,
                                    Number = number
                                });

                                if (passports.Count == sizeCollection)
                                {
                                    yield return new List<Passport>(passports);

                                    returnCount++;

                                    passports.Clear();
                                }
                            }
                        }
                    }
                }
            }

            if (passports.Count > 0)
            {
                yield return passports;

                returnCount++;
            }

            if (returnCount == 0)
            {
                throw new InvalidDataException("Файл Data.csv не содержит необходимые данные.");
            }
        }

        private static bool ValidationCheck(int series, int number)
        {
            if (series == 0 || number == 0)
            {
                return false;
            }

            int seriesDigits = (int)Math.Log10(Math.Abs(series)) + 1;
            int numberDigits = (int)Math.Log10(Math.Abs(number)) + 1;

            if (seriesDigits <= 4 && numberDigits <= 6)
            {
                return true;
            }

            return false;
        }

        private static int LineCountCheck(string path)
        {
            int count = 0;

            using (StreamReader streamReader = new StreamReader(path))
            {
                while (streamReader.ReadLine() != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}