using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace ModelManipulation
{
    internal static class Transforms
    {
        //Transformation matrices
        private static Complex[][] ydTransform;
        private static Complex[][] dyTransform;
        private static Complex[][] A;
        private static Complex[][] inverseA;
        private static Complex[][] vSequenceLLtoLN;
        private static Complex[][] vllToLn;
        private static Complex[][] ungroundedNIline;

        /// <summary>
        /// Method for transforming 3-dimensional vectors
        /// </summary>
        public static Complex[] VectorTransform(Complex[] input, Complex[][] tMatrix)
        {
            var vect = new Complex[3];
            var output = new Complex[3]; 
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //May cause an issue with closure. Pay attention to this during debugging.
                    vect[i] = vect[i] + input[j] * tMatrix[i][j];
                }
                output[i] = vect[i];
            }
            return output;
        }

        /// <summary>
        /// Overload for cases in which the entire transformation matrix must be multiplied by a scalar. 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="tMatrix"></param>
        /// <param name="constantMultiplier"></param>
        /// <returns></returns>
        public static Complex[] VectorTransform(Complex[] input, Complex[][] tMatrix, Complex constantMultiplier)
        {
            var vect = new Complex[3];
            Complex[] output = new Complex[3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //May cause an issue with closure. Pay attention to this during debugging.
                    vect[i] = vect[i] + input[j] * tMatrix[i][j] ;
                }
                output[i] = vect[i] * constantMultiplier;
            }
            return output;
        }



        static Transforms()
        {
            Complex a  = Complex.FromPolarCoordinates(1, Math.PI * (2/3));
            Complex a2 = Complex.FromPolarCoordinates(1, Math.PI * (4/3));
            Complex t = Complex.FromPolarCoordinates(1 / Math.Sqrt(3), Math.PI * (1 / 6));
            //used to transform currents from Wye (line to neutral) to Delta (line to line)
            ydTransform = new Complex[][]
            {
                new Complex[]{1,-1,0},
                new Complex[]{0,1,-1},
                new Complex[]{-1,0,1}
            };
            //used to transform currents from Delta (line to line) to Wye (line to neutral)
            dyTransform = new Complex[][]
            {
                new Complex[]{1,0,-1},
                new Complex[]{-1,1,0},
                new Complex[]{0,-1,1}        
            };
            //used to transform from sequence coordinates into phasors
            A = new Complex[][]
            {
                new Complex[]{1,1,1},
                new Complex[]{1,a2,a},
                new Complex[]{1,a,a2}
            };
            //used to transform from phasors into sequence coordinates
            inverseA = new Complex[][] 
            {
                new Complex[]{1/3,1/3,1/3},
                new Complex[]{1/3,a/3,a2/3},
                new Complex[]{1/3,a2/3,a/3}
            };
            //used to transform line to line sequence voltages to line to neutral
            vSequenceLLtoLN = new Complex[][] 
            {
                new Complex[]{1,0,0},
                new Complex[]{0,Complex.Conjugate(t),0},
                new Complex[]{0,0,t}
            };
            vllToLn = new Complex[][]
            {
                new Complex[]{0,2/3,1/3},
                new Complex[]{1/3,0,2/3},
                new Complex[]{2/3,1/3,0}
            };
            ungroundedNIline = new Complex[][] 
            {
                new Complex[]{1/3,-1/3,0},
                new Complex[]{1/3,2/3,0},
                new Complex[]{-2/3,-1/3,0}
            };
        }
        /// <summary>
        /// Used to obtain the line to neutral voltages for an unbalanced node given the line to line voltages
        /// </summary>
        /// <param name="voltageLL"></param>
        /// <returns></returns>
        public static Complex[] VLLtoLN(Complex[] voltageLL, double turnsRatio)
        {
            return VectorTransform(voltageLL, vllToLn, 1/turnsRatio);
        }
        public static Complex[] VLNtoLL(Complex[] voltageLN, double turnsRatio)
        {
            return VectorTransform(voltageLN, dyTransform, 1 / turnsRatio);
        }

        /// <summary>
        /// Used to obtain the winding currents in a delta connection given the line currents, 
        /// assuming for now that the Y connection is ungrounded.
        /// </summary>
        public static Complex[] ILinetoIWinding(Complex[] lineCurrents)
        {
            return VectorTransform(lineCurrents, ungroundedNIline);         
             
        }
        public static Complex[] IDtoW(Complex[] lineToLineCurrents, double turnsRatio)
        {
            return VectorTransform(lineToLineCurrents, dyTransform,1/turnsRatio);
        }
    }
}
