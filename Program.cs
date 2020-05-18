using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestMiddleDB
{
    class Program
    {
        public List<User> Users;
        public User FindUser { get; set; }
        object sync = new object();

        public static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        public static CancellationToken token = cancelTokenSource.Token;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {
            Console.WriteLine("Press Enter to Start, Press 'ESC' to cancel");
            Console.ReadKey();

            new Task(() =>
            {
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    cancelTokenSource.Cancel();
            }).Start();

            Users = new List<User>();
            ReadFileStream();
            Parallel.ForEach(Users, new ParallelOptions { CancellationToken = token }, user =>
            {
                ListToSQL(user);
            });

        }
        /// <summary>
        /// Десериализация строки в список users
        /// </summary>
        /// <param name="json"></param>
        public void ReadString(string json)
        {
            Users = JsonConvert.DeserializeObject<List<User>>(json);
        }
        /// <summary>
        /// Ведение лога в файл
        /// </summary>
        /// <param name="user"></param>
        /// <param name="Status"></param>
        public void Log(User user, string Status)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"ID: {user.Identity} Name: {user.FIO}  Age: {user.Date} City: {user.City} Status: {Status}");
                w.WriteLine("-------------------------------");
            }
        }
        /// <summary>
        /// Запись List в базу данных
        /// </summary>
        /// <param name="item"></param>
        public void ListToSQL(User item)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Console.WriteLine($"ID: {item.Identity} Name: {item.FIO}  Date: {item.Date}");
                lock (sync)
                {
                    FindUser = db.Users.Find(item.Identity);
                    if (FindUser != null && FindUser.Date > item.Date)
                    {
                        db.Users.Update(item);
                        db.SaveChanges();
                        Log(item, "Update");
                    }
                    if (FindUser == null)
                    {
                        db.Users.Add(item);
                        db.SaveChanges();
                        Log(item, "Add");
                    }
                }
            }
        }
        /// <summary>
        /// Чтение файла в строку и передача на десериализацию
        /// </summary>
        public void ReadFileStream()
        {
            using (StreamReader sr = new StreamReader("user.json", System.Text.Encoding.Default))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    ReadString(line);
                }
            }
        }
    }
}