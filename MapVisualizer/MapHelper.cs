using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace MapVisualizer
{
    public static class MapHelper
    {
        /// <summary>
        /// Gets the city at pixel (in rendered image), might be useful to show info on the image click.
        /// </summary>
        /// <param name="imageSize">Size of the image.</param>
        /// <param name="paddingLeft">The padding left.</param>
        /// <param name="paddingRight">The padding right.</param>
        /// <param name="paddingTop">The padding top.</param>
        /// <param name="paddingBottom">The padding bottom.</param>
        /// <param name="point">The point on the image.</param>
        /// <returns>The <see cref="CityAppearanceInfo"/> which is related to clicked area.</returns>
        public static CityAppearanceInfo GetCityAtPoint(
            Size imageSize,
            int paddingLeft,
            int paddingRight,
            int paddingTop,
            int paddingBottom,
            PointF point)
        {
            using (var geoEntitiesContext = new geoEntities())
            {
                var cityInfos = GetCityInfos(geoEntitiesContext).ToList();
                var transformer = new Transformer(
                    cityInfos,
                    imageSize,
                    paddingLeft,
                    paddingRight,
                    paddingTop,
                    paddingBottom);
                var geoPoint = transformer.TransformBack(point);
                return
                    cityInfos.Where(it => it.Geometry.Contains(geoPoint)).Select(it => it.Appearance).FirstOrDefault();
            }
        }

        /// <summary>
        /// Draws the map of cities provided in the SQL database.
        /// </summary>
        /// <param name="imageSize">Size of the image.</param>
        /// <param name="paddingLeft">The padding left.</param>
        /// <param name="paddingRight">The padding right.</param>
        /// <param name="paddingTop">The padding top.</param>
        /// <param name="paddingBottom">The padding bottom.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="borderPen">The border pen.</param>
        /// <param name="legendItems">The legend items.</param>
        /// <param name="legendCaption">The legend caption.</param>
        /// <param name="legendFont">The legend font.</param>
        /// <param name="legendFontColor">Color of the legend font.</param>
        /// <param name="lineSpacing">The line spacing.</param>
        /// <returns>The rendered image.</returns>
        public static Image DrawMap(
            Size imageSize,
            int paddingLeft,
            int paddingRight,
            int paddingTop,
            int paddingBottom,
            Color backgroundColor,
            Pen borderPen,
            IDictionary<Color, string> legendItems,
            string legendCaption,
            Font legendFont,
            Color legendFontColor,
            float lineSpacing)
        {

            var result = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(backgroundColor);
                using (var geoEntitiesContext = new geoEntities())
                {
                    var cityInfos = GetCityInfos(geoEntitiesContext).ToList();
                    DrawCities(
                        graphics,
                        borderPen,
                        imageSize,
                        paddingLeft,
                        paddingRight,
                        paddingTop,
                        paddingBottom,
                        cityInfos);
                    DrawLegend(
                        graphics,
                        paddingLeft,
                        paddingTop,
                        legendItems,
                        legendCaption,
                        legendFont,
                        legendFontColor,
                        lineSpacing);
                }
            }
            return result;
        }

        #region Transformer 

        private class Transformer
        {
            private readonly float _coeff;

            private int _coordinateSystemId;

            private RectangleF _visibleArea;

            private readonly float _xMax;

            private readonly float _xMin;

            private readonly float _yMax;

            private readonly float _yMin;

            public Transformer(
                IEnumerable<CityInfo> cityInfos,
                SizeF size,
                int paddingLeft,
                int paddingRight,
                int paddingTop,
                int paddingBottom)
            {
                var allVertices =
                    cityInfos.SelectMany(cityVertices => GetGeometryPoints(cityVertices.Geometry.Envelope)).ToList();
                this._xMax = (float) allVertices.Max(point => point.XCoordinate.Value);
                this._xMin = (float) allVertices.Min(point => point.XCoordinate.Value);
                this._yMax = (float) allVertices.Max(point => point.YCoordinate.Value);
                this._yMin = (float) allVertices.Min(point => point.YCoordinate.Value);
                this._coordinateSystemId = allVertices.Select(vert => vert.CoordinateSystemId).Distinct().Single();
                this._visibleArea = new RectangleF(
                    0 + paddingLeft,
                    0 + paddingTop,
                    size.Width - paddingRight - paddingLeft,
                    size.Height - paddingBottom - paddingTop);
                this._coeff = Math.Min(this._visibleArea.Width, this._visibleArea.Height);
            }

            public PointF Transform(DbGeometry pnt)
            {
                if (pnt.XCoordinate != null && pnt.YCoordinate != null)
                {
                    return
                        new PointF(
                            this._visibleArea.Left + this._visibleArea.Width / 2
                            + this._coeff
                            * ( ( (float) pnt.XCoordinate.Value - this._xMin ) / ( this._xMax - this._xMin ) - 0.5f ),
                            this._visibleArea.Top + this._visibleArea.Height / 2
                            - this._coeff
                            * ( ( (float) pnt.YCoordinate.Value - this._yMin ) / ( this._yMax - this._yMin ) - 0.5f ));

                }
                return new PointF(float.NaN, float.NaN);
            }

            public DbGeometry TransformBack(PointF point)
            {
                var xCoordinate = this._xMin
                                  + ( ( point.X - this._visibleArea.Left - this._visibleArea.Width / 2 ) / this._coeff
                                      + 0.5f ) * ( this._xMax - this._xMin );
                var yCoordinate = this._yMin
                                  + ( ( -point.Y + this._visibleArea.Top + this._visibleArea.Height / 2 ) / this._coeff
                                      + 0.5f ) * ( this._yMax - this._yMin );
                return DbGeometry.PointFromText(
                    string.Format("POINT ({0} {1})", xCoordinate, yCoordinate),
                    _coordinateSystemId);
            }
        }

        #endregion Transformer

        #region Private static methods

        private static IEnumerable<DbGeometry> GetGeometryPoints(DbGeometry geometry)
        {
            var pointsCount = geometry.PointCount;
            for (var pointIndex = 1; pointIndex <= pointsCount; pointIndex++)
            {
                var point = geometry.PointAt(pointIndex);
                if (point != null && point.XCoordinate != null && point.YCoordinate != null)
                {
                    yield return point;
                }
            }
        }

        private static void DrawCities(
            Graphics graphics,
            Pen borderPen,
            SizeF size,
            int paddingLeft,
            int paddingRight,
            int paddingTop,
            int paddingBottom,
            IList<CityInfo> cityInfos)
        {
            var transformer = new Transformer(cityInfos, size, paddingLeft, paddingRight, paddingTop, paddingBottom);
            foreach (var cityInfo in cityInfos)
            {
                DrawGeometry(cityInfo.Geometry, cityInfo.Appearance.CityColor, graphics, borderPen, cityInfo, transformer);
                var points = GetGeometryPoints(cityInfo.Geometry.Boundary).Select(it => transformer.Transform(it));
                var minX = points.Min(it => it.X);
                var minY = points.Min(it => it.Y);
                var maxX = points.Max(it => it.X);
                var maxY = points.Max(it => it.Y);
                var width = maxX - minX;
                var height = maxY - minY;
                var measurement = graphics.MeasureString(
                    cityInfo.Appearance.CityName,
                    new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular));
                if (width > measurement.Width && height > measurement.Height) //verify if there is enough room
                {
                    var centerPoint = new PointF((maxX+minX)/2, (maxY+minY)/2);
                    graphics.DrawString(cityInfo.Appearance.CityName, new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular), new SolidBrush(Color.Black), centerPoint.X-width/2, centerPoint.Y-height/2);
                }
            }
        }

        private static void DrawGeometry(
            DbGeometry geometry,
            Color color,
            Graphics graphics,
            Pen borderPen,
            CityInfo cityInfo,
            Transformer transformer)
        {
            var childGeometryCount = geometry.ElementCount ?? 0;
            if (childGeometryCount > 1)
            {
                for (var i = 1; i <= childGeometryCount; i++)
                    DrawGeometry(
                        geometry.ElementAt(i),
                        cityInfo.Appearance.CityColor,
                        graphics,
                        borderPen,
                        cityInfo,
                        transformer);
                return;
            }
            var vertices = GetGeometryPoints(geometry).Select(pnt => transformer.Transform(pnt)).ToArray();
            graphics.FillPolygon(new SolidBrush(color), vertices);
            graphics.DrawPolygon(borderPen, vertices);
        }

        private static void DrawLegend(
            Graphics graphics,
            int paddingLeft,
            int paddingTop,
            IDictionary<Color, string> legendItems,
            string legendCaption,
            Font legendFont,
            Color legendFontColor,
            float lineSpacing)
        {
            graphics.DrawString(legendCaption, legendFont, new SolidBrush(legendFontColor), paddingTop, paddingLeft);
            var captionSize = graphics.MeasureString(legendCaption, legendFont);
            var allColors = legendItems.ToList();
            var allCaptions = legendItems.Values;
            var allCaptionSizes = allCaptions.Select(it => graphics.MeasureString(it, legendFont)).ToList();
            var maxLegendItemWidth = allCaptionSizes.Max(it => it.Width);
            var maxLegendItemHeight = allCaptionSizes.Max(it => it.Height);
            var height = maxLegendItemHeight * lineSpacing;
            for (var i = 0; i < allColors.Count; i++)
            {
                var color = allColors[ i ];
                graphics.DrawString(
                    color.Value,
                    legendFont,
                    new SolidBrush(legendFontColor),
                    paddingLeft,
                    paddingTop + i * height + captionSize.Height * lineSpacing);
                graphics.FillRectangle(
                    new SolidBrush(color.Key),
                    paddingLeft + maxLegendItemWidth + 5,
                    paddingTop + i * height + captionSize.Height * lineSpacing,
                    50,
                    height);
            }
        }

        private static IEnumerable<CityInfo> GetCityInfos(geoEntities geoEntitiesContext)
        {
            //geoEntitiesContext.Database.Connection.Open();
            foreach (var cityItem in geoEntitiesContext.cities.SqlQuery(
                @"SELECT
                   CAST(city_nr AS BIGINT) as id
                  , city_nr as city_nr
                  , city_name as city_name
                  , geometrie_fld as geometrie_fld
                  , show_color as show_color
                  , is_water as is_water
                FROM dbo.city"))
            {
                var geometry = cityItem.geometrie_fld;
                if (geometry.IsValid && (geometry.IsClosed ?? false))
                {
                    yield return new CityInfo
                                     {
                                         Geometry = cityItem.geometrie_fld,
                                         Appearance = new CityAppearanceInfo
                                                          {
                                                              CityName = cityItem.city_name,
                                                              CityColor = ColorTranslator.FromHtml(cityItem.show_color),
                                                          }
                                     };
                }
            }
        }

        #endregion Private static methods
    }
}