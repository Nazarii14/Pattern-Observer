using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using static Program.Program;

namespace Program
{
    public static class Program
    {
        public class SalaryEventArgs : EventArgs
        {
            public string NickName { get; set; }
            public int Age { get; set; }
            public int Salary { get; set; }
        }
        public interface IAgeObserver
        {
            void ObserveAge(object sender, SalaryEventArgs e);
        }
        public class UnderThirtyObserver : IAgeObserver
        {
            private List<SalaryEventArgs> salaryEvents = new List<SalaryEventArgs>();
            public void ObserveAge(object sender, SalaryEventArgs args)
            {
                if (args.Age < 30)
                    salaryEvents.Add(args);
            }

            public void WriteData(string outputFile)
            {
                var querry = from sp in salaryEvents
                             select new
                             {
                                 Nick = sp.NickName,
                                 Age = sp.Age,
                                 Salary = sp.Salary
                             };

                var toWrite = new XElement("UnderThirty",
                    from item in querry
                    select new XElement("Person",
                    new XElement("Nick", item.Nick),
                    new XElement("Age", item.Age),
                    new XElement("Salary", item.Salary)));

                toWrite.Save(outputFile);
            }
        }
        public class OverThirtyObserver : IAgeObserver
        {
            private List<SalaryEventArgs> ageEvents = new List<SalaryEventArgs>();
            public void ObserveAge(object sender, SalaryEventArgs args)
            {
                if (args.Age >= 30)
                    ageEvents.Add(args);
            }
            public void WriteData(string outputFile)
            {
                var querry = from sp in ageEvents
                             select new
                             {
                                 Nick = sp.NickName,
                                 Age = sp.Age,
                                 Salary = sp.Salary
                             };

                var toWrite = new XElement("OverThirty",
                    from item in querry
                    select new XElement("Person",
                    new XElement("Nick", item.Nick),
                    new XElement("Age", item.Age),
                    new XElement("Salary", item.Salary)));
                toWrite.Save(outputFile);
            }
        }

        public class SalaryDetector
        {
            public List<IAgeObserver> Observers = new List<IAgeObserver>();
            public void ReadData(string path)
            {
                var XData = new List<XElement>();
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var root = XElement.Load(fs);
                    XData.Add(root);
                }

                var query = from i in XData.Descendants("Data")
                            select new
                            {
                                Nick = i.Element("Nick").Value,
                                Age = int.Parse(i.Element("Age").Value),
                                Salary = int.Parse(i.Element("Salary").Value),
                            };

                foreach (var item in query)
                {
                    if (item.Salary >= 10000)
                    {
                        var args = new SalaryEventArgs
                        {
                            NickName = item.Nick,
                            Age = item.Age,
                            Salary = item.Salary,
                        };
                        foreach (IAgeObserver observer in Observers)
                        {
                            observer.ObserveAge(this, args);
                        }
                    }
                }
            }

        }
        public static void Main(string[] args)
        {
            //                   Task
            //Працівник характеризується нікнеймом, віком, зарплатою
            //Дані про працівників задано в XML-файлі.
            //Розрізняють дві категорії відповідно до віку: <=30 i > 30
            //Використовуючи патерн Observer вивести у файли дані про кожну категорію, а саме тих, в кого зарплата >= 10k
            //У форматі <nick, age, salary>, посортовано по salary в порядку спадання.

            var path = "data.xml";
            string overThirtyFile = "overThirty.xml";
            string underThirtyFile = "underThirty.xml";

            var detector = new SalaryDetector();
            var overThirtyObs = new OverThirtyObserver();
            var underThirtyObs = new UnderThirtyObserver();

            detector.Observers.Add(overThirtyObs);
            detector.Observers.Add(underThirtyObs);

            detector.ReadData(path);

            overThirtyObs.WriteData(overThirtyFile);
            underThirtyObs.WriteData(underThirtyFile);
        }
    }
}


