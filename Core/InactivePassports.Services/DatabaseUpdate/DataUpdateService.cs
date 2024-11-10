using System.Globalization;
using InactivePassports.Contracts.DatabaseUpdate;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Configurations;
using InactivePassports.Data.Entities.File;
using CsvHelper;

namespace InactivePassports.Services.DatabaseUpdate
{
    public class DataUpdateService : IDataUpdate
    {
        private readonly DataUpdateOptions _options;
        private readonly IDatabaseUpdate _repository;

        private Queue<Passport> _passportsForInsert;
        private Queue<Passport> _passportsForActivation;
        private Queue<Passport> _passportsForDeactivation;

        private HashSet<Passport> _passportsInactive;
        private HashSet<Passport> _passportsActive;
        private HashSet<Passport> _passportsInactiveActual;

        private readonly SemaphoreSlim _semaphore;
        private readonly int _batchSize;

        public DataUpdateService(IDatabaseUpdate databaseUpdateRepository, DataUpdateOptions options)
        {
            _passportsForInsert = new Queue<Passport>();
            _passportsForActivation = new Queue<Passport>();
            _passportsForDeactivation = new Queue<Passport>();
            _passportsInactiveActual = new HashSet<Passport>();

            _options = options;
            _repository = databaseUpdateRepository;

            _semaphore = new SemaphoreSlim(_options.SemaphoreSize);
            _batchSize = _options.BatchSize;
        }

        public void GetData(HashSet<Passport> passportsInactive, HashSet<Passport> passportsActive)
        {
            _passportsInactive = passportsInactive;
            _passportsActive = passportsActive;
        }

        public void UpdateData(IEnumerable<List<Passport>> passports)
        {
            int count = 0;

            foreach (var passport in passports)
            {
                CompareData(passport);

                count++;

                Console.WriteLine(count);
                Console.WriteLine(passport.Count);
            }

            CompareRemainData();
            CopyData();

            Console.WriteLine("Готово");
        }

        private void CompareData(List<Passport> passports)
        {
            List<Task> tasks = new List<Task>();

            foreach (Passport passport in passports)
            {
                if (_passportsActive.Contains(passport))
                {
                    _passportsActive.Remove(passport);
                    _passportsInactiveActual.Add(passport);

                    _passportsForDeactivation.Enqueue(passport);

                    if (_passportsForDeactivation.Count == _batchSize)
                    {
                        //Обновить(деактивировать)
                        Passport[] passportsBatch = new Passport[_batchSize];

                        for (int i = 0; i < _batchSize; i++)
                        {
                            passportsBatch[i] = _passportsForDeactivation.Dequeue();
                        }

                        tasks.Add(Update(passportsBatch, ActionType.Deactivated));
                    }

                    continue;
                }

                if (!_passportsInactive.Contains(passport))
                {
                    _passportsInactiveActual.Add(passport);

                    _passportsForInsert.Enqueue(passport);

                    if (_passportsForInsert.Count == _batchSize)
                    {
                        //Вставить
                        Passport[] passportsBatch = new Passport[_batchSize];

                        for (int i = 0; i < _batchSize; i++)
                        {
                            passportsBatch[i] = _passportsForInsert.Dequeue();
                        }

                        tasks.Add(Insert(passportsBatch));
                    }

                    continue;
                }

                _passportsInactive.Remove(passport);
                _passportsInactiveActual.Add(passport);
            }

            Task.WaitAll(tasks.ToArray());
        }

        private void CompareRemainData()
        {
            List<Task> tasks = new List<Task>();

            foreach (Passport passport in _passportsInactive)
            {
                _passportsInactive.Remove(passport);
                _passportsActive.Add(passport);

                _passportsForActivation.Enqueue(passport);

                if (_passportsForActivation.Count == _batchSize)
                {
                    //Обновить(активировать)
                    Passport[] passportsBatch = new Passport[_batchSize];

                    for (int i = 0; i < _batchSize; i++)
                    {
                        passportsBatch[i] = _passportsForActivation.Dequeue();
                    }

                    tasks.Add(Update(passportsBatch, ActionType.Activated));
                }
            }

            if (_passportsForActivation.Count > 0)
            {
                //Обновить(активировать)
                int count = _passportsForInsert.Count;

                Passport[] passportsBatch = new Passport[count];

                for (int i = 0; i < count; i++)
                {
                    passportsBatch[i] = _passportsForActivation.Dequeue();
                }

                tasks.Add(Update(passportsBatch, ActionType.Activated));
            }

            if (_passportsForDeactivation.Count > 0)
            {
                //Обновить(деактивировать)
                int count = _passportsForInsert.Count;

                Passport[] passportsBatch = new Passport[count];

                for (int i = 0; i < count; i++)
                {
                    passportsBatch[i] = _passportsForDeactivation.Dequeue();
                }

                tasks.Add(Update(passportsBatch, ActionType.Deactivated));
            }

            if (_passportsForInsert.Count > 0)
            {
                //Вставить
                int count = _passportsForInsert.Count;

                Passport[] passportsBatch = new Passport[count];

                for (int i = 0; i < count; i++)
                {
                    passportsBatch[i] = _passportsForInsert.Dequeue();
                }

                tasks.Add(Insert(passportsBatch));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private void CopyData()
        {
            Task inactivePassportsTask = Task.Run(() =>
            {
                string inactivePassportsPath = _options.InactivePassportsPath;

                using (StreamWriter writer = new StreamWriter(inactivePassportsPath))
                {
                    using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(_passportsInactiveActual);
                    }
                }
            });

            Task activePassportsTask = Task.Run(() =>
            {
                string activePassportsPath = _options.ActivePassportsPath;

                using (StreamWriter writer = new StreamWriter(activePassportsPath))
                {
                    using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(_passportsActive);
                    }
                }
            });

            Task.WaitAll(inactivePassportsTask, activePassportsTask);
        }

        private async Task Insert(Passport[] passports)
        {
            await _semaphore.WaitAsync();

            try
            {
                await _repository.Insert(passports, _batchSize);

            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task Update(Passport[] passports, ActionType actionType)
        {
            await _semaphore.WaitAsync();

            try
            {
                await _repository.Update(passports, actionType);

            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}