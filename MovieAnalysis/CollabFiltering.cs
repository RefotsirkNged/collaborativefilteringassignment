using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Numerics;

namespace MovieAnalysis
{
    class CollabFiltering
    {
        private List<Dictionary<Tuple<int, int>, int>> trainingFoldList;

        private List<Dictionary<Tuple<int, int>, int>> testFoldList;

        //Tuple<User, Movie>, Rating
        private Dictionary<Tuple<int, int>, int> dataDictionary;

        private int latentFactors = 25;
        private Matrix<double> RMatrix;
        private Matrix<double> AMatrix;
        private Matrix<double> BMatrix;
        private Matrix<double> sigmaMatrix;


        public CollabFiltering()
        {
            trainingFoldList = new List<Dictionary<Tuple<int, int>, int>>();
            testFoldList = new List<Dictionary<Tuple<int, int>, int>>();
            LoadData();
            

            for (int k = 0; k < 5; k++)
            {
                Console.WriteLine("FOLD NR " + k + "-----------------------------------");

                RMatrix = FillMatrix(trainingFoldList[k]);
                AMatrix = new DenseMatrix(RMatrix.RowCount, latentFactors);
                BMatrix = new DenseMatrix(latentFactors, RMatrix.ColumnCount);
                FunkSVDSetup();

                for (int i = 0; i < 30; i++)
                {
                    FunkSVDTeach(0.1);
                }

                for (int i = 0; i < 200; i++)
                {
                    FunkSVDTeach(0.01);
                }

                for (int i = 0; i < 1000; i++)
                {
                    FunkSVDTeach(0.001);
                }

                Dictionary<Tuple<int, int>, double> results = new Dictionary<Tuple<int, int>, double>();
                foreach (Tuple<int, int> usermovieTuple in testFoldList[k].Keys)
                {

                    results[usermovieTuple] = FunkSVDPredict(usermovieTuple.Item1, usermovieTuple.Item2);

                }

                foreach (Tuple<int, int> usermovieTuple in results.Keys)
                {
                    //Console.WriteLine("User " + usermovieTuple.Item1 + "movie " + usermovieTuple.Item2);
                    //Console.WriteLine("Actual: " + testFoldList[k][usermovieTuple]);
                    //Console.WriteLine("Prediction: " + results[usermovieTuple]);
                }
                int howmanywasright = 0;
                Dictionary<Tuple<int, int>, double> testlist = new Dictionary<Tuple<int, int>, double>();
                List<int> guessedWrongFrequency = new List<int>();
                foreach (Tuple<int, int> usermovietuple in results.Keys)
                {
                    if (results[usermovietuple] >= testFoldList[k][usermovietuple]-0.75 && results[usermovietuple] <= testFoldList[k][usermovietuple] + 0.75)
                    {
                        howmanywasright++;
                    }
                    else
                    {
                        testlist.Add(usermovietuple, results[usermovietuple]);
                        guessedWrongFrequency.Add(usermovietuple.Item2);
                    }
                }
                //Console.WriteLine("This many was nearly right: " + howmanywasright);
                //Console.WriteLine("out of " + results.Keys.Count);
            }
        }

        private double GetStochasticGradientDescentA(int movie, int k)
        {
            Random rand = new Random();
            int randNum = rand.Next(0, RMatrix.ColumnCount);
            double rating;
            while (RMatrix[movie, randNum] == 0)
            {
                randNum = rand.Next(0, RMatrix.ColumnCount);
            }

            rating = RMatrix[movie, randNum];
            double sum = 0;
            for (int i = 0; i < latentFactors; i++)
            {
                sum += AMatrix[movie, i] * BMatrix[i, randNum];
            }
            return (rating - sum) * BMatrix[k, randNum];
        }

        private double GetStochasticGradientDescentB(int user, int k)
        {
            Random rand = new Random();
            int randNum = rand.Next(0, RMatrix.RowCount);
            double rating;
            while (RMatrix[randNum, user] == 0)
            {
                user = rand.Next(0, RMatrix.ColumnCount);
            }

            rating = RMatrix[randNum, user];
            double sum = 0;
            for (int i = 0; i < latentFactors; i++)
            {
                sum += AMatrix[randNum, i] * BMatrix[i, user];
            }
            return AMatrix[randNum, k] * (rating - sum);

        }

        private double FunkSVDPredict(int user, int movie)
        {
            
            double predictedRating = 0;

            for (int i = 0; i < latentFactors; i++)
            {
                predictedRating += AMatrix[user-1, i] * BMatrix[i, movie-1];
            }
            return predictedRating;
        }

        private void FunkSVDTeach(double stepSize)
        {
            for (int i = 0; i < latentFactors * 2; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < AMatrix.RowCount; j++)
                    {
                        AMatrix[j, i / 2] += stepSize * GetStochasticGradientDescentA(j, i/2);
                    }
                }
                else
                {
                    for (int j = 0; j < AMatrix.RowCount; j++)
                    {
                        BMatrix[i / 2, j] += stepSize * GetStochasticGradientDescentB(j, i/2);
                    }
                }
            }
        }

        private void FunkSVDSetup()
        {
            Control.UseNativeMKL();
            Control.UseMultiThreading();
            Matrix<double> mmtMatrix = RMatrix * (RMatrix.Transpose());
            Evd<double> UEigenvectors = mmtMatrix.Evd();
            Matrix<double> mtmMatrix = RMatrix.Transpose() * RMatrix;
            Evd<double> VEigenvectors = mtmMatrix.Evd();
            sigmaMatrix = new DiagonalMatrix(RMatrix.RowCount, RMatrix.ColumnCount);

            for (int i = 0; i < RMatrix.RowCount; i++)
            {
                sigmaMatrix[i, i] = Math.Sqrt(UEigenvectors.EigenValues[i].Real);
            }

            Matrix<double> USigma = UEigenvectors.EigenVectors * sigmaMatrix;
            Matrix<double> VSigma = sigmaMatrix * VEigenvectors.EigenVectors.Transpose();

            for (int i = 0; i < RMatrix.RowCount; i++)
            {
                for (int j = 0; j < latentFactors; j++)
                {
                    AMatrix[i, j] = USigma[i, j];
                }
            }
            
            for (int i = 0; i < latentFactors; i++)
            {
                for (int j = 0; j < RMatrix.ColumnCount; j++)
                {
                    BMatrix[i, j] = VSigma[i, j];
                }
            }
            


            //Console.WriteLine(sigmaMatrix);
            //Console.WriteLine(VSigma);
            //Console.WriteLine(USigma);
            //Console.WriteLine(AMatrix);
            //Console.WriteLine(BMatrix);
        }

        private Matrix<double> FillMatrix(Dictionary<Tuple<int, int>, int> input)
        {
            HashSet<int> users = new HashSet<int>();
            HashSet<int> movies = new HashSet<int>();
            foreach (Tuple<int, int> userMoviePair in input.Keys)
            {
                users.Add(userMoviePair.Item1);
                movies.Add(userMoviePair.Item2);
            }

            Matrix<double> result = new DenseMatrix(users.OrderByDescending(m => m).ToList().First(),
                movies.OrderByDescending(m => m).ToList().First());
            foreach (Tuple<int, int> userMoviePair in input.Keys)
            {
                result[userMoviePair.Item1 - 1, userMoviePair.Item2 - 1] = input[userMoviePair];
            }
            return result;
        }

        private void LoadData()
        {
            string dataFolder = "ml-100k";
            for (int i = 0; i < 5; i++)
            {
                string trainingPath = dataFolder + "//u" + (i + 1) + ".base";
                string testPath = dataFolder + "//u" + (i + 1) + ".test";
                Dictionary<Tuple<int, int>, int> trainingData = ReadDataFile(trainingPath);
                Dictionary<Tuple<int, int>, int> testData = ReadDataFile(testPath);

                foreach (Tuple<int, int> usermoviepairs in testData.Keys)
                {
                    trainingData[usermoviepairs] = 0;
                }
                trainingFoldList.Add(trainingData);
                testFoldList.Add(testData);
            }
        }

        private Dictionary<Tuple<int, int>, int> ReadDataFile(string filePath)
        {
            Dictionary<Tuple<int, int>, int> result = new Dictionary<Tuple<int, int>, int>();
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

                            result[new Tuple<int, int>(Int32.Parse(lineContents[0]), Int32.Parse(lineContents[1]))] =
                                Int32.Parse(lineContents[2]);
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