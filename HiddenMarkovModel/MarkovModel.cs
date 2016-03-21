using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenMarkovModel
{
    class MarkovModel
    {
        private HiddenMarkovClassifier<MultivariateNormalDistribution> hmm;
        private Dictionary<string, int> ActivityIndex = new Dictionary<string, int>()
        {
            {"rinsing mouth with water",0},
            {"brushing teeth",1},
            {"wearing contact lenses",2},
            {"talking on the phone",3},
            {"drinking water",4},
            {"opening pill container",5},
            {"cooking (chopping)",6},
            {"cooking (stirring)",7},
            {"talking on couch",8},
            {"relaxing on couch",9},
            {"writing on whiteboard",10},
            {"working on computer",11}
        };
        private int NumberOfActivity = 12;


        private int NumberOfFeature =2;
        private int[][] input { get; set; }
        private int[] output { get; set; }
        public MarkovModel()
        {
            DirectoryInfo d = new DirectoryInfo("GMMFile");
            FileInfo[] files = d.GetFiles("*.txt");
            input = new int[files.Count()][];
            output = new int[files.Count()];
            for (int i = 0; i < files.Count(); ++i)
            {
                var fileStream = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    line = streamReader.ReadLine();
                    int valueOut;
                    if (ActivityIndex.ContainsKey(line))
                    {
                        valueOut = ActivityIndex[line];
                        output[i] = valueOut;
                    }
                    line = streamReader.ReadLine();
                    int NumberofSequence = int.Parse(line);
                    input[i] = new int[NumberofSequence];
                    for (int j = 0; j < NumberofSequence; j++)
                    {
                        line = streamReader.ReadLine();
                        int val = int.Parse(line);
                        input[i][j] = val;
                    }

                }
            }
        }
        public void Run()
        {
            int states = 3;
            int iterations = 0;
            double tolerance = 0.01;
            bool rejection = false;
            string[] classes = ActivityIndex.Keys.ToArray();
           
            hmm = new HiddenMarkovClassifier<MultivariateNormalDistribution>(classes.Length,
               new Forward(states), new MultivariateNormalDistribution(NumberOfFeature), classes);
            // Create the learning algorithm for the ensemble classifier
            var teacher = new HiddenMarkovClassifierLearning<MultivariateNormalDistribution>(hmm,

                // Train each model using the selected convergence criteria
                i => new BaumWelchLearning<MultivariateNormalDistribution>(hmm.Models[i])
                {
                    Tolerance = tolerance,
                    Iterations = iterations,

                    FittingOptions = new NormalOptions()
                    {
                        Regularization = 1e-5
                    }
                  
                    
                }
            );
            teacher.Empirical = true;
            teacher.Rejection = rejection;


            // Run the learning algorithm
            double error = teacher.Run(input, output);
            //foreach (var sample in database.Samples)
            //{
            //    sample.RecognizedAs = hmm.Compute(sample.Input);
            //}

        }
        
    }
}
