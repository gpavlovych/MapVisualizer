using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Web;
using MapVisualizer;

namespace TestGeometryDrawWebApplication
{
    /// <summary>
    /// Summary description for DrawMap
    /// </summary>
    public class DrawMap : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var legendCaption = "Legenda";
            var legend = new Dictionary<Color, string>()
                             {
                                 { ColorTranslator.FromHtml("#63BE7B"), ">95%" },
                                 { ColorTranslator.FromHtml("#BDD881"), "90-95%" },
                                 { ColorTranslator.FromHtml("#E9E583"), "60-90%" },
                                 { ColorTranslator.FromHtml("#FA8E72"), "30-60%" },
                                 { ColorTranslator.FromHtml("#E15151"), "<30%" },
                                 { ColorTranslator.FromHtml("#BFBFBF"), "Onbekend" }
                             };
            var widthStr = context.Request["width"] ?? "";
            var heightStr = context.Request["height"] ?? "";
            int width;
            int height;
            if (!int.TryParse(widthStr, out width))
                width = 500;
            if (!int.TryParse(heightStr, out height))
                height = 500;
            var mode = context.Request[ "mode" ] ?? "";
            if (mode == "draw")
            {
                var font = new Font(FontFamily.GenericMonospace, 8, FontStyle.Bold);
                var legendFontColor = Color.Black;
                using (
                    var result = MapHelper.DrawMap(
                        new Size(width, height),
                        50,
                        50,
                        50,
                        50,
                        Color.White,
                        new Pen(Color.Black, 1.0f),
                        legend,
                        legendCaption,
                        font,
                        legendFontColor,
                        1.5f))
                {
                    // set MIME type
                    context.Response.ContentType = "image/png";

                    // write to response stream
                    result.Save(context.Response.OutputStream, ImageFormat.Png);
                }
            }
            else if (mode == "getinfo")
            {

                var xStr = context.Request["x"] ?? "";
                var yStr = context.Request["y"] ?? "";
                float x;
                float y;
                if (!float.TryParse(xStr, out x))
                    x = 0;
                if (!float.TryParse(yStr, out y))
                    y = 0;
                var city = MapHelper.GetCityAtPoint(
               new Size(width, height),
               50,
               50,
               50,
               50,
               new PointF(x, y));
                if (city != null)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat("City Name: {0}", city.CityName);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendFormat("Value: {0}", legend[city.CityColor]);
                    {
                        // set MIME type
                        context.Response.ContentType = "text/plain";

                        // write to response stream
                        context.Response.Write(stringBuilder.ToString());
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}