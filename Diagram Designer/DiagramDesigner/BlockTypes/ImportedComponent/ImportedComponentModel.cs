using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Xml;
using DiagramDesigner.Model;
using DiagramDesigner.sNpFile;
using MathNet.Numerics.LinearAlgebra;

namespace DiagramDesigner.BlockTypes.ImportedComponent
{
    public class ImportedComponentModel : Element
    {
        public List<Matrix<Complex>> SMatrices { get; set; }
        public List<double> Frequencies { get; set; }
        public string FileName { get; set; }

        private double _referenceImpedance;

        public ImportedComponentModel()
        {
            Type = ElementType.ImportedComponent;
            Name = "Imported Component";
        }

        public void LoadFromTouchstoneResult(TouchstoneResult touchstoneResult)
        {
            SMatrices = touchstoneResult.SMatrices;
            Frequencies = touchstoneResult.Frequencies;
            _referenceImpedance = touchstoneResult.ReferenceImpedance;
            _dimension = SMatrices[0].ColumnCount; //port count
            S = Matrix<Complex>.Build.Dense(_dimension, _dimension);
            CreateEvenlySpacedPorts(_dimension);
        }

        private void CreateEvenlySpacedPorts(int portCount)
        {
            if (portCount > 0)
            {
                int rightHalf = portCount / 2;
                int leftHalf = portCount - rightHalf;
                for (int i = 0; i < leftHalf; i++)
                {
                    double vertPos = (double)i / leftHalf + 0.5 / leftHalf;
                    CreateConnector(0, vertPos, ConnectorOrientation.Left);
                }
                for (int i = 0; i < rightHalf; i++)
                {
                    double vertPos = (double)i / rightHalf + 0.5 / rightHalf;
                    CreateConnector(1, vertPos, ConnectorOrientation.Right);
                }
            }
        }


        private int _indexOfLastFrequency = 0;
        public override bool UpdateScatteringMatrix(double frequency, Complex referenceImpedance)
        {
            List<int> Result;
            if (Frequencies[_indexOfLastFrequency] <= frequency && Frequencies.Count > _indexOfLastFrequency + 1)
            {
                if (Frequencies[_indexOfLastFrequency + 1] > frequency)
                {
                    LinearInterpolation(_indexOfLastFrequency, _indexOfLastFrequency + 1, frequency); //computes interpolated S Matrix
                    return true;
                }
                Result = FindNearestPointsForInterpolation(Frequencies, _indexOfLastFrequency, frequency);
            }
            else
            {
                Result = FindNearestPointsForInterpolation(Frequencies, 0, frequency);
            }

            switch (Result.Count)
            {
                case 1:
                    S = SMatrices[Result[0]].Clone();
                    _indexOfLastFrequency = Result[0];
                    break;
                case 2:
                    LinearInterpolation(Result[0], Result[1], frequency);
                    _indexOfLastFrequency = Result[0];
                    break;
                default:
                    throw new Exception("Found Points are not proper");
            }

            return true;
        }

        protected override void ReadUniqueProperties(XmlReader reader)
        {
            if (reader.Read() && reader.Name == "FileName") FileName = reader.ReadString();
            else throw new Exception("FileName of ImportedComponentModel is missing or it is in the wrong place");
            ReadSMatrices(reader);
        }

        protected override void WriteUniqueProperties(XmlWriter writer)
        {
            writer.WriteElementString("FileName", FileName);
            WriteSMatrices(writer);
        }

        private void WriteSMatrices(XmlWriter writer)
        {
            writer.WriteStartElement("File");
            writer.WriteElementString("Dimension", Dimension.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ReferenceImpedance", _referenceImpedance.ToString(CultureInfo.InvariantCulture));
            for (int i = 0; i < Frequencies.Count; i++)
            {
                writer.WriteElementString("F", Frequencies[i].ToString(CultureInfo.InvariantCulture));
                for (int j = 0; j < Dimension; j++)
                {
                    for (int k = 0; k < Dimension; k++)
                    {
                        writer.WriteElementString("SR", SMatrices[i][j, k].Real.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("SI", SMatrices[i][j, k].Imaginary.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            writer.WriteEndElement();
        }

        private void ReadSMatrices(XmlReader reader)
        {
            if (reader.Read() && reader.NodeType == XmlNodeType.Element && reader.Name == "File")
            {
                if (reader.Read() && reader.Name == "Dimension") _dimension = int.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                else throw new Exception("Dimension of ImportedComponentModel is missing or it is in the wrong place");

                if (reader.Read() && reader.Name == "ReferenceImpedance") _referenceImpedance = double.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                else throw new Exception("ReferenceImpedance of ImportedComponentModel is missing or it is in the wrong place");

                SMatrices = new List<Matrix<Complex>>();
                Frequencies = new List<double>();
                reader.Read();
                while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "File"))
                {
                    if (reader.Name == "F") Frequencies.Add(double.Parse(reader.ReadString(), CultureInfo.InvariantCulture));
                    else throw new Exception("Frequency of ImportedComponentModel is missing or it is in the wrong place");
                    Matrix<Complex> SMatrix = Matrix<Complex>.Build.Dense(Dimension, Dimension);
                    SMatrices.Add(SMatrix);
                    for (int i = 0; i < Dimension; i++)
                    {
                        for (int j = 0; j < Dimension; j++)
                        {
                            double real;
                            double imag;
                            if (reader.Read() && reader.Name == "SR")
                            {
                                real = double.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                            }
                            else throw new Exception("Scattering Matrix component of ImportedComponentModel is missing or it is in the wrong place");

                            if (reader.Read() && reader.Name == "SI")
                            {
                                imag = double.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                            }
                            else throw new Exception("Scattering Matrix component of ImportedComponentModel is missing or it is in the wrong place");
                            SMatrix[i, j] = new Complex(real, imag);
                        }
                    }
                    reader.Read();
                }
                S = Matrix<Complex>.Build.Dense(_dimension, _dimension);
            }
        }

        #region Helper Methods

        private void LinearInterpolation(int leftPointIndex, int rightPointIndex, double interpolationPoint)
        {
            //interpolation formula S(x) = S(x0) + [S(x1)-S(x0)] / [x1-x0] * [x-x0]
            double denom = Frequencies[rightPointIndex] - Frequencies[leftPointIndex]; //[x1-x0]
            double numerator = interpolationPoint - Frequencies[leftPointIndex]; //[x-x0]
            Matrix<Complex> leftSideSMatrix = SMatrices[leftPointIndex]; //S(x0)
            Matrix<Complex> rightSideSMatrix = SMatrices[rightPointIndex]; //S(x1)
            for (var i = 0; i < Dimension; i++)
            {
                for (var j = 0; j < Dimension; j++)
                {
                    var phase = leftSideSMatrix[i, j].Phase + ((rightSideSMatrix[i, j].Phase - leftSideSMatrix[i, j].Phase) * numerator) / denom;
                    var magnitude = leftSideSMatrix[i, j].Magnitude + ((rightSideSMatrix[i, j].Magnitude - leftSideSMatrix[i, j].Magnitude) * numerator) / denom;
                    S[i, j] = Complex.FromPolarCoordinates(magnitude, phase);
                }
            }
        }

        private List<int> FindNearestPointsForInterpolation(List<double> freqsList, int fromIndex, double frequency) //returns indexes of two nearest points, or if it is exactly that frequency it returns one point
        {
            var L = fromIndex;
            var R = freqsList.Count - 1;
            if (frequency <= freqsList[L])
            {
                return new List<int> { L }; //below range
            }
            if (frequency >= freqsList[R])
            {
                return new List<int> { R }; //above range
            }

            //modified binary search
            while (L <= R)
            {
                var m = (L + R) / 2;
                if (freqsList[m] < frequency)
                {
                    if (freqsList[m + 1] > frequency)
                    {
                        return new List<int> { m, m + 1 };
                    }
                    L = m;
                }
                else if (freqsList[m] > frequency)
                {
                    if (freqsList[m - 1] < frequency)
                    {
                        return new List<int> { m - 1, m };
                    }
                    R = m;
                }
                else return new List<int> { m };
            }
            throw new Exception("Frequencies Array in " + FileName + " is not sorted");
        }

        #endregion
    }
}