using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieAnalysis
{
    class CollabFiltering
    {
        private List<List<Review>> trainingDataList;
        private List<List<Review>> testDataList;


        public CollabFiltering()
        {
            trainingDataList = new List<List<Review>>();
            testDataList = new List<List<Review>>();
            LoadData();
        }



        public void LoadData()
        {
            string dataFolder = "ml-100k";
            for (int i = 0; i < 5; i++)
            {
                string trainingPath = dataFolder + "//u" + (i+1) + ".base";
                string testPath = dataFolder + "//u" + (i+1) + ".test";
                List<Review> trainingData = ReadDataFile(trainingPath);
                List<Review> testData = ReadDataFile(testPath);
                trainingDataList.Add(trainingData);
                testDataList.Add(testData);
            }
        }


        public static List<Review> ReadDataFile(string filePath)
        {
            var result = new List<Review>();
            try
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line == String.Empty)
                        {
                            Console.WriteLine("File is empty.");
                        }
                        else
                        {
                            string[] lineContents = line.Split('\t');
                            if (lineContents.Length != 4)
                            {
                                Console.WriteLine("File has the wrong format.");
                            }

                            result.Add(new Review(Int32.Parse(lineContents[0]), Int32.Parse(lineContents[1]), Int32.Parse(lineContents[2]), Int32.Parse(lineContents[3])));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }
    }
}