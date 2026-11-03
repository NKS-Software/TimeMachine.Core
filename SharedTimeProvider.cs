using System.IO.MemoryMappedFiles;

#pragma warning disable
namespace TimeMachine.Core
{
    public static class SharedTimeProvider
    {
        private static readonly Mutex Mutex;
        private static readonly MemoryMappedFile Mmf;
        private static readonly MemoryMappedViewAccessor Accessor;
        private static bool _isInitialized = false;

        static SharedTimeProvider()
        {
            try
            {
                Mutex = new Mutex(false, Constants.MutexName);

                Mmf = MemoryMappedFile.CreateOrOpen(Constants.Name, sizeof(long));
                Accessor = Mmf.CreateViewAccessor();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации разделяемой памяти: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _isInitialized = false;
            }
        }

        public static void Write(DateTime dateTime)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Память не инициализирована");

            var l = dateTime.ToBinary();
            bool hasHandle = false;

            try
            {
                hasHandle = Mutex.WaitOne(3000); 

                if (!hasHandle)
                {
                    return;
                }

                Accessor.Write(0, l);
            }
            catch (AbandonedMutexException)
            {
                Console.WriteLine("Обнаружен abandoned мьютекс, продолжаем запись");
                Accessor.Write(0, l);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Непредвиденная ошибка:");
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (hasHandle)
                    Mutex.ReleaseMutex();
            }
        }

        public static DateTime Read(DateTime? defaultValue = null)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Память не инициализирована");

            bool hasHandle = false;
            try
            {
                hasHandle = Mutex.WaitOne(3000);

                if (!hasHandle)
                {
                    return !defaultValue.HasValue ? DateTime.MinValue : defaultValue.Value;
                }

                var l = Accessor.ReadInt64(0);
                var result = DateTime.FromBinary(l);
                return result != DateTime.MinValue || !defaultValue.HasValue ? result : defaultValue.Value;
            }
            catch (AbandonedMutexException)
            {
                Console.WriteLine("Обнаружен abandoned мьютекс, продолжаем чтение");
                var l = Accessor.ReadInt64(0);
                return DateTime.FromBinary(l);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Непредвиденная ошибка:");
                Console.WriteLine(ex.ToString());
                return DateTime.MinValue;
            }
            finally
            {
                if (hasHandle)
                    Mutex.ReleaseMutex();
            }
        }
    }
}