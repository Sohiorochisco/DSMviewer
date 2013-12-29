using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Numerics;

namespace IdentifiedObjects
{
    /// <summary>
    /// LineModel Is the parent class for the basic line model types ( not found in CIM)
    /// </summary>

    public class LineModel : IdentifiedObject
    {
        public PhaseCode Phasing { get;private set; }
        /// <summary>
        /// Indicates whether the matrices have been kron reduced
        /// </summary>
        public bool Reduced { get; private set; }
        
        public LineModel(DataRow objFields) : base(objFields) 
        {
            Phasing = (PhaseCode)Convert.ToInt32(objFields["Phases"]);
            Reduced = (bool)objFields["Reduced"];
            initializeArrays(objFields);
        }

        public float[][] B{get;private set;}
        public float[][] X{get;private set;}
        public float[][] R{get;private set;}

        private Complex[][] zPerLengthMatrix;
        public Complex[][] PhaseImpedancePerLength()
        {
            if (zPerLengthMatrix == null)
            {
                zPerLengthMatrix = new Complex[3][] { new Complex[3], new Complex[3], new Complex[3] };
                //Shift is used to account for the fact that while Z and R are upper triangle, since they are stored in a jagged array the indexes
                // are off of what would be expected for an upper triangle by n - 1, where n is the row number
                int shift = 0;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {

                        if (j >= i)
                        {
                            zPerLengthMatrix[i][j] =  R[i][j - shift] * Complex.One + X[i][j - shift] * Complex.ImaginaryOne;
                        }
                        else
                        {
                            zPerLengthMatrix[i][j] =  R[j][i - shift] * Complex.One + X[j][i - shift] * Complex.ImaginaryOne;
                        }

                    }
                    shift++;
                }
            }

            return zPerLengthMatrix;

        }
        /// <summary>
        /// Gives capacitance values derived from the stored susceptance values.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public float C(int i, int k)
        {
            return (float)(B[i][k] * (1/( 120 * Math.PI)) * 1E9);
        }

        protected void initializeArrays(DataRow data)
        {
            if (Reduced)
            {
                B = new float[][]
            {
                new float[]
                {
                    (float)(double)data["b11"],
                    (float)(double)data["b12"],
                    (float)(double)data["b13"]
                },
                new float[]
                {
                    (float)(double)data["b22"],
                    (float)(double)data["b23"]
                },
                new float[]
                {
                    (float)(double)data["b33"]
                }
                
            };
                R = new float[][]
            {
                new float[]
                {
                    (float)(double)data["r11"],
                    (float)(double)data["r12"],
                    (float)(double)data["r13"]
                },
                new float[]
                {
                    (float)(double)data["r22"],
                    (float)(double)data["r23"]
                },
                new float[]
                {
                    (float)(double)data["r33"]
                }
                
            };

                X = new float[][]
            {
                new float[]
                {
                    (float)(double)data["x11"],
                    (float)(double)data["x12"],
                    (float)(double)data["x13"]
                },
                new float[]
                {
                    (float)(double)data["x22"],
                    (float)(double)data["x23"]
                },
                new float[]
                {
                    (float)(double)data["x33"]
                }
                
            };


            }
            else
            {
                B = new float[][]
            {
                new float[]
                {
                    (float)(double)data["b11"],
                    (float)(double)data["b12"],
                    (float)(double)data["b13"],
                    (float)(double)data["b14"]
                },
                new float[]
                {
                    (float)(double)data["b22"],
                    (float)(double)data["b23"],
                    (float)(double)data["b24"]
                },
                new float[]
                {
                    (float)(double)data["b33"],
                    (float)(double)data["b34"]
                },
                new float[]
                {
                    (float)(double)data["b44"]
                }
                
            };
                R = new float[][]
            {
                new float[]
                {
                    (float)(double)data["r11"],
                    (float)(double)data["r12"],
                    (float)(double)data["r13"],
                    (float)(double)data["r14"]
                },
                new float[]
                {
                    (float)(double)data["r22"],
                    (float)(double)data["r23"],
                    (float)(double)data["r24"]
                },
                new float[]
                {
                    (float)(double)data["r33"],
                    (float)(double)data["r34"]
                },
                new float[]
                {
                   (float)(double)data["r44"]
                }
                
            };

                X = new float[][]
            {
                new float[]
                {
                    (float)(double)data["x11"],
                    (float)(double)data["x12"],
                    (float)(double)data["x13"],
                    (float)(double)data["x14"]
                },
                new float[]
                {
                    (float)(double)data["x22"],
                    (float)(double)data["x23"],
                    (float)(double)data["x24"]
                },
                new float[]
                {
                    (float)(double)data["x33"],
                    (float)(double)data["x34"]
                },
                new float[]
                {
                    (float)(double)data["x44"]
                }
                
            };
            }

        }

        
    }
}
