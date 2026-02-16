using System.Drawing;

namespace MapVisualizer
{
    /// <summary>
    /// Container for city information: city name and city color in the map.
    /// </summary>
    public class CityAppearanceInfo
    {
        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        /// <value>
        /// The name of the city.
        /// </value>
        public string CityName { get; set; }

        /// <summary>
        /// Gets or sets the color of the city.
        /// </summary>
        /// <value>
        /// The color of the city.
        /// </value>
        public Color CityColor { get; set; }
    }
}