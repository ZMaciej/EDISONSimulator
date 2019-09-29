using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using DiagramDesigner.BlockTypes.Port;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.Win32;
using OxyPlot;

namespace DiagramDesigner.Model
{
    /// <summary>
    /// An object containing connected elements (chain)
    /// Object also solves the chain
    /// </summary>
    public class ElementsChain
    {
        public List<Element> Elements { get; } = new List<Element>();
        public Dictionary<int, int> ConnectionList { get; } = new Dictionary<int, int>(); //list of connections, tells which ports of S matrix are connected with each other. Ports are ordered as "element number + port of Element number"
        public List<int> ConnectorsSum { get; } = new List<int>(); //first element with its S matrix starts on W matrix at ConnectorsSum[0] index, second element at ConnectorsSum[1] an so on
        public Dictionary<int, PortModel> PortsIndexes { get; } = new Dictionary<int, PortModel>();
        public List<PortModel> Ports { get; } = new List<PortModel>();
        private SparseMatrix _w; //circuit matrix
        private SparseMatrix _gamma; //connection matrix
        private Vector<Complex> _e;
        public List<ScatteringMatrixSample> S { get; } = new List<ScatteringMatrixSample>();
        public List<List<List<Complex>>> SPlotsMatrix { get; } = new List<List<List<Complex>>>(); //first two lists are the two dimensions of matrix; the list inside matrix is data that can be plotted
        public List<double> Frequencies { get; } = new List<double>();

        public void Solve(double fromFrequency, double toFrequency, double frequencyStep, Complex referenceImpedance)
        {
            var fromFreq = fromFrequency;
            var toFreq = toFrequency;
            var stepFreq = frequencyStep;
            var currentFreq = fromFrequency;
            var refImpedance = referenceImpedance;
            _w = SparseMatrix.Create(ConnectorsSum.Last(), ConnectorsSum.Last(), 0);
            _gamma = SparseMatrix.Create(ConnectorsSum.Last(), ConnectorsSum.Last(),0);
            _e = Vector<Complex>.Build.Dense(ConnectorsSum.Last());

            //initializing SPlotMatrix
            for (int i = 0; i < PortsIndexes.Count; i++)
            {
                SPlotsMatrix.Add(new List<List<Complex>>()); //initializing first dimension
                for (int j = 0; j < PortsIndexes.Count; j++)
                {
                    SPlotsMatrix[i].Add(new List<Complex>()); //initializing second dimension
                }
            }

            //setting up connection matrix
            foreach (KeyValuePair<int, int> pair in ConnectionList)
            {
                _w[pair.Key, pair.Value] = 1;
                _w[pair.Value, pair.Key] = 1;
                _gamma[pair.Key, pair.Value] = 1;
                _gamma[pair.Value, pair.Key] = 1;
            }

            bool firstPort = true;
            for (int j = 0; j < PortsIndexes.Count; j++)
            {
                var activePort = PortsIndexes.ElementAt(j);
                _e.Clear();
                _e[activePort.Key] = 1;
                currentFreq = fromFrequency;

                //calculating response to each port
                Complex portImpedance = new Complex(activePort.Value.PortImpedance, 0);
                _e = _e.Multiply(Complex.Sqrt(portImpedance) / (refImpedance + portImpedance.Conjugate())); // E * c

                bool firstIteration = true;
                if (fromFreq < toFreq && stepFreq > 0)
                {
                    int frequencyIterator = 0;
                    while (currentFreq <= toFreq)
                    {
                        for (int k = 0; k < Elements.Count; k++) //for each element update its scattering matrix and pass it into W matrix
                        {
                            Element element = Elements[k];
                            if (element.UpdateScatteringMatrix(currentFreq, refImpedance) || firstIteration)
                            {
                                SparseMatrix temp;
                                temp = SparseMatrix.OfMatrix(element.ScatteringMatrix);
                                temp.Multiply(-1);
                                _w.SetSubMatrix(ConnectorsSum[k], ConnectorsSum[k],temp);
                            }
                        }

                        firstIteration = false;
                        /* a = */
                        Vector<Complex> resultVectorA = _w.Solve(_e); //solve circuit for current frequency
                        /* b = */
                        Vector<Complex> resultVectorB = _gamma.Multiply(resultVectorA);
                        /* s11 = a1./b1 */

                        if (firstPort) //if it is firstPort scattering matrices should be initialized
                        {
                            S.Add(new ScatteringMatrixSample(currentFreq, Matrix<Complex>.Build.Dense(PortsIndexes.Count, PortsIndexes.Count)));
                            Frequencies.Add(currentFreq/1000000000); //Hz to GHz
                        }
                        for (int i = 0; i < PortsIndexes.Count; i++) //fill one row of a chain Scattering matrix
                        {
                            var otherPort = PortsIndexes.ElementAt(i);
                            S[frequencyIterator].ScatteringMatrix[i, j] = resultVectorA[otherPort.Key] / resultVectorB[activePort.Key]; //Sij = ai./bj
                            SPlotsMatrix[i][j].Add(S[frequencyIterator].ScatteringMatrix[i, j]);
                        }

                        if (currentFreq < toFreq)
                        {
                            currentFreq += stepFreq;
                            currentFreq = currentFreq > toFreq ? toFreq : currentFreq;
                            frequencyIterator++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    firstPort = false;
                }
            }
        }
    }

    public class ComplexPoint
    {
        public Complex Complex { get; }
        public double Frequency { get; }

        public ComplexPoint(double frequency, Complex complex)
        {
            Frequency = frequency;
            Complex = complex;
        }
    }

    public class ScatteringMatrixSample
    {
        public Matrix<Complex> ScatteringMatrix { get; }
        public double Frequency { get; }

        public ScatteringMatrixSample(double frequency, Matrix<Complex> scatteringMatrix)
        {
            Frequency = frequency;
            ScatteringMatrix = scatteringMatrix;
        }
    }
}