/*using System.Collections;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using static NetworkClasses.Operations;

namespace NetworkClasses
{
    static class MyExt
    {
        static Operations operations = new Operations();  

        public static double calculate(this string str, double x, double y)
        {
            return operations.calculations[str](x, y);
        }

        public static MathOperation calculate(this string str)
        {
            return operations.calculations[str];
        }
    }

    class Murray<T> : IEnumerator<T>
    {
        public int[] Dimensions;
        T[] Data;
        int current = 0;

        Murray(int[] dimension, Type t) {
            Dimensions = dimension;
            Data = new T[Dimensions.Aggregate((x, y) => x * y)];
        }

        Murray(ref Array array)
        {
            int ranks = array.Rank;
            int[] Dimensions = new int[ranks];

            for (int i = 0; i < ranks; i++)
            {
                Dimensions[i] = array.GetLength(i);
            }

            Data = array.Cast<T>().ToArray();
        }

        public T Current => Data[current];

        public int[] Coordinates => Dimensions.Take();

        object IEnumerator.Current => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    class network
    {
        public List<layer> Layers { get; set; }

        network() { }
        public double[] PassForward(double[] inp)
        {
            foreach (layer l in Layers)
            {
                inp = l.passForward(inp);
            }
            return inp;
        }
    }

    class layer
    {
        bool hasBias;
        double[]? Bias;
        double[,] Weights;
        string Operation;
        string LossFunction = "MeanSquared";
        double[][,] activations = new double[0][,];


        public layer(int inpSize, int outSize, string operation="*")
        {
            Operation = operation;
            Weights = new double[outSize, inpSize];
            for (int i = 0; i < outSize; i++)
            {
                Weights. = new double[inpSize];
            }
        }

        public layer(double[,] weights, double[]? bias, string operation = "*")
        {
            Operation = operation;
            hasBias = bias != null;
            Weights = weights;
            Bias = bias;
        }

        public void targetPassBackward(double[][] targets, int batchSize, double learningRate, string lossFunction = "MeanSquared")
        {
            double[,,] dWeights = new double[batchSize, activations[0].Length, activations[0][0].Length];
            for (int i = 0; i < batchSize; i++)
            {
                for (int j = 0; j < activations[0].Length; j++)
                {
                    for (int k = 0; k < activations[0][0].Length; k++)
                    {
                        double error = lossFunction.calculate(activations[i][j][k], targets[i][j]);
                        dWeights[i, j, k] = error;
                    }
                }
            }
        }

        public void passBackward(double[] previouseError, int batchSize, double learningRate = 1)
        {
            for (int i = 0; i < activations[0].Length; i++)
            {
                for (int j = 0; j < batchSize; j++)
                {

                }
            }
            activations = new double[0][];
            //activation(1-activation)(weight*error previous)
        }

        public void ChangeWeights(double[][][] dWeights, int batchSize, double learningRate)
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[0].Length; j++)
                {
                    Weights[i][j] +=  dWeights * "Log".calculate(3, batchSize) * learningRate;
                }
            }
        }

        public double[] passForward(double[] inp)
        {
            double[] output = new double[inp.Length];
            int cur = activations.Length;
            activations.Append(new double[inp.Length][]);

            for (int i = 0; i < inp.Length; i++)
            {
                activations[cur][i] = new double[Weights.Length];
                for (int j = 0; j < Weights.Length; j++)
                {
                    activations[cur][i][j] = Operation.calculate(inp[j], Weights[i][j]);
                }
                output[i] = activations[cur][i].Sum();
                if (Bias != null)
                {
                    output[i] += Bias[i];
                }
            }
            return output;
        }
    }

    class Operations
    {
        public delegate double MathOperation(double x, double y);

        public Dictionary<string, MathOperation> calculations = new Dictionary<string, MathOperation>();

        public Operations()
        {
            calculations.Add("+", delegate (double x, double y) { return x + y; });
            calculations.Add("-", delegate (double x, double y) { return x - y; });
            calculations.Add("*", delegate (double x, double y) { return x * y; });
            calculations.Add("/", delegate (double x, double y) { return x / y; });
            calculations.Add("^", delegate (double x, double y) { return Math.Pow(x, y); });
            calculations.Add("Log", delegate (double x, double y) { return Math.Log(x, y); });
            calculations.Add("MeanSquared", delegate (double x, double y) { return Math.Pow((x - y), 2); });
        }
    }
}
*/