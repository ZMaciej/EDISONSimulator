using System;
using System.Numerics;
using DiagramDesigner.Model;
using MathNet.Numerics.LinearAlgebra;

namespace DiagramDesigner.BlockTypes.OpenCircuit
{
    public class OpenCircuitModel : Element
    {
        public OpenCircuitModel()
        {
            Type = ElementType.OpenCircuit;
            Name = "Open Circuit";
            CreateConnector(0, 0.5, ConnectorOrientation.Left);

            _dimension = 1;
            S = Matrix<Complex>.Build.Dense(_dimension, _dimension);
            S[0, 0] = new Complex(1, 0);
        }

        public override Boolean UpdateScatteringMatrix(double frequency, Complex referenceImpedance)
        {
            return false; //returns false when the ScatteringMatrix stays the same
        }
    }
}