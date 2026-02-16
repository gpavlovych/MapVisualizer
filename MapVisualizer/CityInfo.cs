using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Drawing;

namespace MapVisualizer
{
    internal class CityInfo
    {
        public CityAppearanceInfo Appearance { get; set; }

        public DbGeometry Geometry { get; internal set; }
    }
}