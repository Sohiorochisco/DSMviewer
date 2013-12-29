using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace IdentifiedObjects
{
    /// <summary>
    /// Represents a single point in space
    /// </summary>
    public class PositionPoint : IdentifiedObject
    {
        static private string coordinateSystem;
        /// <summary>
        /// CoordinateSystem gives the Uniform Resource Name for the Coordinate Reference System used by position points
        /// </summary>
        static public string CoordinateSystem { get { return coordinateSystem; } }
        private float xPosition;
        private float yPosition;
        /// <summary>
        /// Point Latitude
        /// </summary>
        public float YPosition { get { return yPosition; } }
        /// <summary>
        /// Point Longitude
        /// </summary>
        public float XPosition { get { return xPosition; } }

        public PositionPoint(DataRow row)
            : base(row)
        {
            xPosition = (float)(double)row["Xposition"];
            yPosition = (float)(double)row["Yposition"];

        }
    }
}
