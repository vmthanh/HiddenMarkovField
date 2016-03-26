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
        private HiddenMarkovClassifier hmm;
        //Activity name
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
        //private int NumberOfFeature =2;
        private int[][] input { get; set; }
        private int[][] inpuTest { get; set; }
        private int[] output { get; set; }
        private int[] outpuTest { get; set; }
        private string[] outputTestLabelFolder { get; set; }
        private void loadTrain(){
            /*Initialize data
            * Read the Folder and get the data form each file
            * Each file begin with the Name of Activity and then the number of data point
            * In those next line, each line is a number of each data point
            * */
            DirectoryInfo d = new DirectoryInfo("GMMFileTrain");
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
        private void loadTest()
        {
            DirectoryInfo d = new DirectoryInfo("GMMTest");
            FileInfo[] files = d.GetFiles("*.txt");
            inpuTest = new int[files.Count()][];
            outpuTest = new int[files.Count()];
            outputTestLabelFolder = new string[files.Count()];
            for (int i = 0; i < files.Count(); ++i)
            {
                string fileLabelName = files[i].Name;
                outputTestLabelFolder[i] = fileLabelName;
                var fileStream = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    line = streamReader.ReadLine();
                    int valueOut;
                    if (ActivityIndex.ContainsKey(line))
                    {
                        valueOut = ActivityIndex[line];
                        outpuTest[i] = valueOut;
                    }
                    line = streamReader.ReadLine();
                    int NumberofSequence = int.Parse(line);
                    inpuTest[i] = new int[NumberofSequence];
                    for (int j = 0; j < NumberofSequence; j++)
                    {
                        line = streamReader.ReadLine();
                        int val = int.Parse(line);
                        inpuTest[i][j] = val;
                    }

                }
            }


        }
        public MarkovModel()
        {
            loadTrain();
            loadTest();

        }
       
        public void Run()
        {
            /*Initialize the model
             * Read more tut on Code project for better understanding
             * http://www.codeproject.com/Articles/541428/Sequence-Classifiers-in-Csharp-Part-I-Hidden-Marko?msg=5219822#xx5219822xx
             * states is parameters for running forward algo
             * intteration is parameters for iterations
             * tolerance is parameters for threshold
             * */
            int states = 3;
            int iterations = 100;
            double tolerance = 0.01;
            bool rejection = false;
            string[] classes = ActivityIndex.Keys.ToArray();
            ITopology foward =  new Forward(states:3);

            hmm = new HiddenMarkovClassifier(classes: 12, topology: foward, symbols: 5);
            // Create the learning algorithm for the ensemble classifier
            var teacher = new HiddenMarkovClassifierLearning(hmm,

                // Train each model using the selected convergence criteria
                i => new BaumWelchLearning(hmm.Models[i])
                {
                    Tolerance = tolerance,
                    Iterations = iterations,

                         
                    
                }
            );
            teacher.Empirical = true;
            teacher.Rejection = rejection;


            // Run the learning algorithm
            double error = teacher.Run(input, output);
            Console.WriteLine("Error: {0}", error);
            //Run the test and compare the real value
            using(StreamWriter writer = new StreamWriter("compare.txt"))
            {
                for(int i=0;i<outpuTest.Length; ++i)
                 {
                      int val = hmm.Compute(inpuTest[i]);
                     if (val != outpuTest[i]){
                         string labelTestRetrieve = ActivityIndex.FirstOrDefault(x => x.Value == val).Key;
                         string labelTestActity = ActivityIndex.FirstOrDefault(x => x.Value == outpuTest[i]).Key;
                         writer.WriteLine(outputTestLabelFolder[i] + " - " +"false, label test retrieve: "+labelTestRetrieve +" label activity: "+labelTestActity);
                     }
                     else{
                         string labelTestActity = ActivityIndex.FirstOrDefault(x => x.Value == outpuTest[i]).Key;
                         writer.WriteLine(outputTestLabelFolder[i] + " - " + "true, label activity: " + labelTestActity);
                     }
                 }
                
            }
            

        }
        
    }
}
