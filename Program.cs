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
        public static List<User> Users;
        public User FindUser { get; set; }

        public static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        public static CancellationToken token = cancelTokenSource.Token;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {
            new Task(() =>
            {
                if(Console.ReadKey().Key == ConsoleKey.Escape)
                cancelTokenSource.Cancel();
            }).Start();

            Users = new List<User>();
            ParallelLoopResult loopResult = ProcessFile("user.json");
            Console.WriteLine(loopResult.IsCompleted);
            if (loopResult.IsCompleted == true)
            {
                foreach (User item in Users)
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        Console.WriteLine($"Name: {item.FIO}  Age: {item.Date}");
                        FindUser = db.Users.Find(item.Identity);
                        if (FindUser == null)
                        {
                            db.Users.Add(item);
                            db.SaveChanges();
                            Log(item);
                        }
                        if(FindUser != null && FindUser.Date > item.Date)
                        {
                            db.Users.Update(item);
                            db.SaveChanges();
                            Log(item);
                        }
                    }
                }
            }
        }

        public static void ReadString(string json)
        {
            Users = JsonConvert.DeserializeObject<List<User>>(json);
        }

        public static ParallelLoopResult ProcessFile(string path)
        {
            ParallelLoopResult loopResult = Parallel.ForEach(File.ReadLines(path), 
                new ParallelOptions { CancellationToken = token }, line =>
            {
                ReadString(line);
            });
            return loopResult;
        }

        public void Log(User user)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"Name: {user.FIO}  Age: {user.Date} City: {user.City}");
                w.WriteLine("-------------------------------");
            }
        }
    }
}
