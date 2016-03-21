using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Models.Markov;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenMarkovModel
{
    class Program
    {
       

        static void Main(string[] args)
        {
            MarkovModel markov = new MarkovModel();
            markov.Run();
        }
        
        
    }
}
