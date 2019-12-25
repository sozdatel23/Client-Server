using System;
using System.IO;

namespace Network_communication
{
    class Logger
    {
        private StreamWriter writer;
        private string path;

        private static readonly object writeLock = new object();

        public string GetPath()
        {
            return path;
        }

      
        public Logger(string path)
        {
            this.path = path;
            try
            {
                if (!File.Exists(path))
                {
                    writer = File.CreateText(path);
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Невозможно открыть лог файл. Исключение: {0}", e.Message);
            }
        }

      
        public void Log(string message)
        {
            Console.WriteLine(message);

            string date = DateTime.Now.ToString("HH:mm:ss");
            lock (writeLock)
            {
                try
                {
                    writer = File.AppendText(path);
                    writer.WriteLine("{0} {1}", date, message);
                    writer.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при запуске файла. Исключение: {0}", e.Message);
                }
            }
        }

    }
}
