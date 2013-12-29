using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace ModelManipulation
{
    static class MatrixSolvers
    {
        static public Tuple<Complex[][],Complex[][]> LUDecomposition(Complex[][] B, Complex[] Y)
        {
            var  length = Y.GetLength(0);
            if (length != B.GetLength(0) || B.GetLength(1) != length)
            {
                throw new Exception("The matrix and vector do not have the same dimension");
            }
            
            var U = new Complex[length][];
            var L = new Complex[length][];
            Complex sum1;
            Complex sum2;
            Complex b0i = 0;
            //Initialize the Jagged arrays for Doolittle L and U
            for (int i = 0; i < length; i++)
            {
                L[i] = new Complex[length];
                U[i] = new Complex[length];
                b0i = B[0][i];
                U[0][i] = b0i;
                L[i][0] = B[i][0] / b0i;
                L[i][i] = 1;
            }
            //Perform the decomposition - currently no pivoting, very computationally intensive
            for (int i = 1; i < length; i++)
            {
                for (int j = i; j < length; j++)
                {
                    sum1 = 0;
                    sum2 = 0;
                    for (int k = 0; k < j; k++)
                    {
                        sum1 += L[i][k] * U[k][j];
                        sum2 += L[j][k] * U[k][i];
                    }
                    U[i][j] = B[i][j] - sum1;
                    L[j][i] = (B[j][i] - sum2) / U[i][j];
                }
                                
            }

            return new Tuple<Complex[][], Complex[][]>(L, U);
 
        }

        public static Complex[] FBSubstitution(Complex[][] L,Complex[][]U, Complex[] y )
        {
            var length = L.GetLength(0);
            var interm  = new Complex[length];
            var x = new Complex[length];
            Complex sum = 0;
            //Substitute y values into L to get the intermediate vector
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    sum += y[j] * L[i][j];
                }
                interm[i] = y[i] - sum;
            }

            //Substitute intermediate vector values into U to get x
            for (int i = length - 1; i >= 0; i--)
            {
                for (int j = i; j < i; j++)
                {
                    sum += y[j] * L[i][j];
                }
                interm[i] = y[i] - sum;
 
            }
            return y;
        }

    }
}
