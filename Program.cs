using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestMiddleDB
{
    class Program
    {
        public static List<User> models;
        public User FindUser { get; set; }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {

            models = new List<User>();
            ParallelLoopResult loopResult = ProcessFile("user.json");
            Console.WriteLine(loopResult.IsCompleted);
            if (loopResult.IsCompleted == true)
            {
                foreach (User item in models)
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        Console.WriteLine($"Name: {item.FIO}  Age: {item.Date}");
                        FindUser = db.Users.Find(item.Identity);
                        if (FindUser == null)
                        {
                            //db.Models.Find(item)
                            db.Users.Add(item);
                            db.SaveChanges();
                        }

                    }
                }

            }
        }

        public static void ReadString(string json)
        {
            models = JsonConvert.DeserializeObject<List<User>>(json);
            //models.Add(model);
            //Console.WriteLine($"Name: {model.FIO}  Age: {model.Date}");
        }

        public static ParallelLoopResult ProcessFile(string path)
        {
            ParallelLoopResult loopResult = Parallel.ForEach(File.ReadLines(path), line =>
            {
                ReadString(line);
            });
            return loopResult;
        }
    }
}
