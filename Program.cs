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

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

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
                        }
                        if(FindUser.Date > item.Date)
                        {
                            db.Users.Update(item);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public void ReadString(string json)
        {
            Users = JsonConvert.DeserializeObject<List<User>>(json);
        }

        public ParallelLoopResult ProcessFile(string path)
        {
            ParallelLoopResult loopResult = Parallel.ForEach(File.ReadLines(path), line =>
            {
                ReadString(line);
            });
            return loopResult;
        }
    }
}
