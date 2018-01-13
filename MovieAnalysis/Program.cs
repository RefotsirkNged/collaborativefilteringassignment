using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieAnalysis
{
    //User based CF, the alternative is Item based, does not fit very well, we dont know much about the item? pr thats what i think
    //user based: compute how similar to user A user B is, and use that to calculate what user A would rate movie XY (a movie user A has not watched before)
    //To build item-based, we need info about the item, which we do not have much of (only a name)

    class Program
    {
        static void Main(string[] args)
        {
            var result = LoadData();
        }




        public static bool LoadData()
        {

            List<Review> trainingData = new List<Review>();
            List<Review> testData = new List<Review>();
            List<int> trainingDataWithoutUserAndMovie = SubtractUserAndMovieFromDataset(trainingData);


            string dataFolder = "ml-100k";
            string trainingPath = dataFolder + "//u1.base";
            string testPath = dataFolder + "//u1.test";
            trainingData = ReadDataFile(trainingPath);
            testData = ReadDataFile(testPath);
            return true;
        }


        public static void ConstructMatrix(List<Review> dataset, int minFactors, int maxFactors)
        {
            /* 
             • 𝑈𝑚: total number of observed ratings for movie 𝑚
             • 𝑀𝑢: total number of observed ratings for user 𝑢
             • 𝑁: total number of observed movie-user pairs.
             * Rmu: rating (movie, user)
             
             
             */






        }




        //i dont know what this is for
        public static List<int> SubtractUserAndMovieFromDataset(List<Review> dataset)
        {
            List<int> result = new List<int>();
            foreach (var review in dataset)
            {
                result.Add(review.Score);
            }

            return result;
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
