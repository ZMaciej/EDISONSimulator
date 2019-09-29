using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using DiagramDesigner.Annotations;
using MathNet.Numerics.LinearAlgebra;

namespace DiagramDesigner.sNpFile
{
    public class TouchstoneResult
    {
        public List<Matrix<Complex>> SMatrices { get; set; }
        public List<double> Frequencies { get; set; }
        public double ReferenceImpedance { get; set; }
        public string FileName { get; set; }
        private int _dimension;

        public TouchstoneResult(int portCount)
        {
            SMatrices = new List<Matrix<Complex>>();
            Frequencies = new List<double>();
            _dimension = portCount;
        }
        public void AddMatrixSample(Matrix<Complex> SMatrix, double Frequency)
        {
            if (SMatrix.ColumnCount == _dimension && SMatrix.RowCount == _dimension)
            {
                SMatrices.Add(SMatrix);
            }
            else
            {
                throw new Exception("Invalid matrix size");
            }
        }
    }

    public class TouchstoneFile
    {
        #region Private fields

        private const string _formatName = "Touchstone";
        private static readonly string[] _extensions = { ".s1p", ".s2p", ".s3p", ".s4p", ".s5p", ".s6p", ".s7p", ".s8p", ".s9p", ".s10p", ".s11p", ".s12p" };
        private static readonly char ExportSeparator = ' ';
        private static readonly char[] ImportSeparators = new[] { ' ', '\t' };
        private TouchstoneResult _result;

        #endregion

        #region Public properties

        public enum DataFormat
        {
            [Description("dB-angle")] DB,
            [Description("Magnitude-angle")] MA,
            [Description("Real-imaginary")] RI
        }

        public enum FrequencyUnit
        {
            GHz,
            MHz,
            kHz,
            Hz,
            None
        }

        public enum ResultDataType
        {
            Scattering,
            Impedance,
            Admittance
        }

        private static List<KeyValuePair<string, FrequencyUnit>> Frequencies = new List<KeyValuePair<string, FrequencyUnit>>()
        {
            new KeyValuePair<string, FrequencyUnit>("GHz", FrequencyUnit.GHz),
            new KeyValuePair<string, FrequencyUnit>("MHz",FrequencyUnit.MHz),
            new KeyValuePair<string, FrequencyUnit>("kHz",FrequencyUnit.kHz),
            new KeyValuePair<string, FrequencyUnit>("Hz",FrequencyUnit.Hz),
        };

        public string FormatName
        {
            get { return _formatName; }
        }

        public string[] Extensions
        {
            get { return _extensions; }
        }

        public TouchstoneOptions Options { get; private set; }

        #endregion

        #region Constructors

        public TouchstoneFile()
        {
            Options = new TouchstoneOptions()
            {
                DataFormat = DataFormat.RI,
                FrequencyUnit = FrequencyUnit.GHz
            };
        }

        #endregion

        #region Import/Export

        public TouchstoneResult Read(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            TouchstoneResult result = null;
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
                result = Read(fs);
            return result;
        }

        public TouchstoneResult Read(Stream stream)
        {
            DataFormat dataFormat;
            FrequencyUnit frequencyUnit;
            double normR;
            List<double> data = new List<double>(); // Stores all double values
            List<int> dataSize = new List<int>(); // Stores number of values in each line


            using (TextReader tr = new StreamReader(stream))
            {
                string line = tr.ReadLine();
                while (line != null)
                {
                    if (line.StartsWith("#"))
                        break;
                    line = tr.ReadLine();
                }

                // Parse options line
                ParseOptionsLine(TrimComments(line), out frequencyUnit, out dataFormat, out normR);

                line = tr.ReadLine();
                while (line != null)
                {
                    line = TrimComments(line);
                    if (line == string.Empty)
                    {
                        line = tr.ReadLine();
                        continue;
                    }

                    string[] s = line.Split(ImportSeparators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string s2 in s)
                    {
                        double d = double.Parse(s2, NumberStyles.Any, CultureInfo.InvariantCulture);
                        data.Add(d);
                    }

                    dataSize.Add(s.Length);
                    line = tr.ReadLine();
                }
            }

            int nPoints = dataSize.Count(p => p % 2 == 1);  //odd number lines count. Means how many frequencies are in the file
            int nSeries = (dataSize.Sum() - nPoints) / (nPoints * 2); //how many complex values belong to one frequency
            int nPorts = (int)Math.Sqrt(nSeries); //number of ports of saved component


            TouchstoneResult result = new TouchstoneResult(nPorts);

            int currentValuePosition = 0;

            for (int i = 0; i < nPoints; i++) //for each frequency
            {
                result.Frequencies.Add(GetHz(data[currentValuePosition++], frequencyUnit));

                Matrix<Complex> newSMatrix = Matrix<Complex>.Build.Dense(nPorts, nPorts);
                for (int j = 0; j < nPorts; j++)
                {
                    for (int k = 0; k < nPorts; k++)
                    {
                        //due to documentation: https://ibis.org/connector/touchstone_spec11.pdf access 30/05/2019
                        if (nPorts == 2)
                            newSMatrix[k, j] = GetComplexNumber(data[currentValuePosition++], data[currentValuePosition++], dataFormat);
                        else
                            newSMatrix[j, k] = GetComplexNumber(data[currentValuePosition++], data[currentValuePosition++], dataFormat);
                    }
                }
                result.SMatrices.Add(newSMatrix);
            }

            result.ReferenceImpedance = normR;
            return result;
        }

        public void Write(List<List<List<Complex>>> SMatrices, List<double> freqs, Complex referenceImpedance, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");


            using (TextWriter tw = new StreamWriter(fileName))
            {
                Write(SMatrices, freqs, referenceImpedance, tw);
            }
        }

        public void Write(List<List<List<Complex>>> SMatrices, List<double> freqs, Complex referenceImpedance, TextWriter tw)
        {
            int maxStringSubLength = 23;
            char subStringFiller = ' ';

            tw.WriteLine("! Exported from EDISON Simulator");
            tw.WriteLine("# " + GetFrequencyUnitSymbol(Options.FrequencyUnit) + " " + GetDataTypeSymbol(ResultDataType.Scattering) + " " + Options.DataFormat + " R " + referenceImpedance.Real.ToString(CultureInfo.InvariantCulture));
            int nPorts;
            if (SMatrices.Count == SMatrices[0].Count) //if first dim equals second dim
            {
                nPorts = SMatrices.Count;
            }
            else
            {
                throw new Exception("Invalid S Matrix dimensions");
            }

            if (nPorts == 0)
                throw new Exception("No data to export.");

            StringBuilder sb = new StringBuilder(1024);

            for (int i = 0; i < freqs.Count; i++)
            {
                sb.Clear();
                sb.Append(ToConstantLengthString(freqs[i], maxStringSubLength, subStringFiller));
                sb.Append(ExportSeparator);
                for (int j = 0; j < nPorts; j++)
                {
                    bool beenCutted = false;
                    int writtenInCurrentRow = 0;
                    for (int k = 0; k < nPorts; k++)
                    {
                        int fi; //first index
                        int si; //second index
                                //indexes are flipped when port count is 2
                        if (nPorts == 2)
                        {
                            fi = k;
                            si = j;
                        }
                        else
                        {
                            fi = j;
                            si = k;
                        }

                        if (writtenInCurrentRow > 3) //due to documentation one row can contain max 4 complex values + 1 frequency value
                        {
                            if (!beenCutted)
                            {
                                sb.Append("!row " + (j + 1).ToString(CultureInfo.InvariantCulture));
                                beenCutted = true;
                            }
                            tw.WriteLine(sb.ToString());
                            sb.Clear();
                            sb.Append(ConstantLengthStringOfChar(subStringFiller, maxStringSubLength + 1));
                            writtenInCurrentRow = 0;
                        }

                        if (SMatrices[fi][si].Count > 0) //if that connection exist
                        {
                            sb.Append(ToConstantLengthString(SMatrices[fi][si][i].Real, maxStringSubLength, subStringFiller));
                            sb.Append(ExportSeparator);
                            sb.Append(ToConstantLengthString(SMatrices[fi][si][i].Imaginary, maxStringSubLength, subStringFiller));
                            sb.Append(ExportSeparator);
                        }
                        else
                        {
                            sb.Append(ToConstantLengthString(0, maxStringSubLength, subStringFiller));
                            sb.Append(ExportSeparator);
                            sb.Append(ToConstantLengthString(0, maxStringSubLength, subStringFiller));
                            sb.Append(ExportSeparator);
                        }
                        writtenInCurrentRow++;
                    }

                    if (nPorts != 2)
                    {
                        tw.WriteLine(sb.ToString());
                        sb.Clear();
                        sb.Append(ConstantLengthStringOfChar(subStringFiller, maxStringSubLength + 1));
                    }
                }
                if (!string.IsNullOrWhiteSpace(sb.ToString()))
                {
                    tw.WriteLine(sb.ToString());
                    sb.Clear();
                }
            }
        }

        public void Write(List<Matrix<Complex>> SMatrices, List<double> freqs, Complex referenceImpedance, TextWriter tw)
        {
            int maxStringSubLength = 23;
            char subStringFiller = ' ';

            tw.WriteLine("! Exported from EDISON Simulator");
            tw.WriteLine("# " + GetFrequencyUnitSymbol(Options.FrequencyUnit) + " " + GetDataTypeSymbol(ResultDataType.Scattering) + " " + Options.DataFormat + " R " + referenceImpedance.Real.ToString(CultureInfo.InvariantCulture));
            int nPorts = SMatrices[0].ColumnCount;

            if (nPorts == 0)
                throw new Exception("No data to export.");

            StringBuilder sb = new StringBuilder(1024);

            for (int i = 0; i < freqs.Count; i++)
            {
                sb.Clear();
                sb.Append(ToConstantLengthString(freqs[i], maxStringSubLength, subStringFiller));
                sb.Append(ExportSeparator);
                for (int j = 0; j < nPorts; j++)
                {
                    bool beenCutted = false;
                    int writtenInCurrentRow = 0;
                    for (int k = 0; k < nPorts; k++)
                    {
                        int fi; //first index
                        int si; //second index
                                //indexes are flipped when port count is 2
                        if (nPorts == 2)
                        {
                            fi = k;
                            si = j;
                        }
                        else
                        {
                            fi = j;
                            si = k;
                        }

                        if (writtenInCurrentRow > 3) //due to documentation one row can contain max 4 complex values + 1 frequency value
                        {
                            if (!beenCutted)
                            {
                                sb.Append("!row " + (j + 1).ToString(CultureInfo.InvariantCulture));
                                beenCutted = true;
                            }
                            tw.WriteLine(sb.ToString());
                            sb.Clear();
                            sb.Append(ConstantLengthStringOfChar(subStringFiller, maxStringSubLength + 1));
                            writtenInCurrentRow = 0;
                        }

                        sb.Append(ToConstantLengthString(SMatrices[i][fi,si].Real, maxStringSubLength, subStringFiller));
                        sb.Append(ExportSeparator);
                        sb.Append(ToConstantLengthString(SMatrices[i][fi, si].Imaginary, maxStringSubLength, subStringFiller));
                        sb.Append(ExportSeparator);
                        writtenInCurrentRow++;
                    }

                    if (nPorts != 2)
                    {
                        tw.WriteLine(sb.ToString());
                        sb.Clear();
                        sb.Append(ConstantLengthStringOfChar(subStringFiller, maxStringSubLength + 1));
                    }
                }
                if (!string.IsNullOrWhiteSpace(sb.ToString()))
                {
                    tw.WriteLine(sb.ToString());
                    sb.Clear();
                }
            }
        }
        #endregion

        #region Helper methods

        public static bool ExtensionIsValid([NotNull]string path)
        {
            var extension = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
            return Regex.IsMatch(extension, @"[.][s]\d+[p]") || extension == ".snp";
        }

        private static void ParseOptionsLine(string line, out FrequencyUnit frequencyUnit, out DataFormat dataFormat, out double normR)
        {
            line = line.Substring(1); // Remove #
            string[] s = line.Split(ImportSeparators, StringSplitOptions.RemoveEmptyEntries);

            if (s.Length != 5)
                throw new Exception("Wrong options file format.");

            frequencyUnit = FrequencyUnit.None;
            foreach (KeyValuePair<string, FrequencyUnit> pair in Frequencies)
            {
                if (s[0].ToLower().Equals(pair.Key.ToLower()))
                    frequencyUnit = pair.Value;
            }
            if (frequencyUnit == FrequencyUnit.None)
                throw new Exception("Unknown or unsupported frequency unit " + s[0]);

            if (!s[1].Equals("S"))
                throw new Exception("Unknown or unsupported data type " + s[1]);

            switch (s[2].ToLower())
            {
                case "ri":
                    dataFormat = DataFormat.RI;
                    break;
                case "ma":
                    dataFormat = DataFormat.MA;
                    break;
                case "db":
                    dataFormat = DataFormat.DB;
                    break;
                default:
                    throw new Exception("Unknown or unsupported data format " + s[2]);
            }

            if (!s[3].Equals("R"))
                throw new Exception("Wrong options file format.");

            normR = double.Parse(s[4], NumberStyles.Any, CultureInfo.InvariantCulture); //refImpedance
        }

        private static double[] GetComplexParts(Complex c, DataFormat dataFormat)
        {
            if (dataFormat == DataFormat.DB)
            {
                return new[] { 20 * Math.Log10(c.Magnitude), c.Phase * 180 / Math.PI };
            }
            if (dataFormat == DataFormat.MA)
            {
                return new[] { c.Magnitude, c.Phase * 180 / Math.PI };
            }
            if (dataFormat == DataFormat.RI)
            {
                return new[] { c.Real, c.Imaginary };
            }
            return null;
        }

        private static Complex GetComplexNumber(double p1, double p2, DataFormat dataFormat)
        {
            if (dataFormat == DataFormat.DB)
            {
                return Complex.FromPolarCoordinates(Math.Pow(10, p1 / 20), p2 * Math.PI / 180);
            }
            if (dataFormat == DataFormat.MA)
            {
                return Complex.FromPolarCoordinates(p1, p2 * Math.PI / 180);
            }
            if (dataFormat == DataFormat.RI)
            {
                return new Complex(p1, p2);
            }
            return new Complex();
        }

        private static double GetHz(double value, FrequencyUnit unit)
        {
            switch (unit)
            {
                case FrequencyUnit.GHz:
                    return value * 1000000000;
                case FrequencyUnit.MHz:
                    return value * 1000000;
                case FrequencyUnit.kHz:
                    return value * 1000;
                case FrequencyUnit.Hz:
                    return value;
                default:
                    throw new Exception("frequency unit is unknown");
            }
        }

        private static string GetDataTypeSymbol(ResultDataType dataType)
        {
            if (dataType == ResultDataType.Scattering)
                return "S";
            if (dataType == ResultDataType.Impedance)
                return "Z";
            if (dataType == ResultDataType.Admittance)
                return "Y";
            return string.Empty;
        }

        private static string GetFrequencyUnitSymbol(FrequencyUnit freqUnit)
        {
            switch (freqUnit)
            {
                case FrequencyUnit.GHz:
                    return "GHz";
                case FrequencyUnit.MHz:
                    return "MHz";
                case FrequencyUnit.kHz:
                    return "kHz";
                case FrequencyUnit.Hz:
                    return "Hz";
                default:
                    throw new Exception("Unknown or unsupported frequency unit ");
            }
        }

        private int GetNumberOfPorts(string fileName)
        {
            using (TextReader tr = new StreamReader(fileName))
            {
                // Skip options line and any comment lines
                string line = tr.ReadLine();
                if (line == null)
                    throw new Exception("No data found.");
                while (true)
                {
                    line = tr.ReadLine();
                    if (line == null)
                        throw new Exception("No data found.");
                    if (!line.StartsWith("#") && !line.StartsWith("!"))
                        break;
                }

                string firstDataLine = TrimComments(line);
                string[] numbers = firstDataLine.Split(ImportSeparators, StringSplitOptions.RemoveEmptyEntries);

                if (numbers.Length < 3)
                    throw new Exception("No data found.");
                if (numbers.Length == 3)
                    return 1;
                if (numbers.Length == 7)
                    return 3;

                List<int> numberOfNumbers = new List<int>();


            }
            return 0;
        }

        private static string TrimComments(string line)
        {
            int index = line.IndexOf('!');
            if (index == 0)
                return string.Empty;
            if (index > -1)
                return line.Substring(0, index);
            return line;
        }

        private static string ToConstantLengthString(double value, int length, char fillSymbol)
        {
            var toReturn = value.ToString(CultureInfo.InvariantCulture);
            while (toReturn.Length < length)
            {
                toReturn += fillSymbol;
            }
            return toReturn;
        }

        private static string ConstantLengthStringOfChar(char fillSymbol, int length)
        {
            return new string(fillSymbol, length);
        }

        #endregion

        #region Options

        public class TouchstoneOptions
        {
            public DataFormat DataFormat { get; set; }
            public FrequencyUnit FrequencyUnit { get; set; }
        }

        #endregion
    }
}