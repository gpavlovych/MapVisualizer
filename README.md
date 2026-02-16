# MapVisualizer

Clickable map visualization library for ASP.NET WebForms and WinForms applications

## Overview

MapVisualizer is a .NET library that provides interactive map visualization capabilities for desktop and web applications. It allows you to render geographic data from SQL databases with support for clickable regions, color-coded areas, and customizable legends. The library includes Entity Framework integration for seamless database access and supports both WinForms and ASP.NET WebForms platforms.

## Features

- **Interactive Map Rendering**: Draw maps with polygons representing cities or regions
- **Click Detection**: Identify which region was clicked on the rendered map
- **Customizable Appearance**: Configure colors, borders, fonts, and padding
- **Legend Support**: Display color-coded legends with custom captions
- **Database Integration**: Built-in Entity Framework support for geographic data
- **Cross-Platform**: Works with both WinForms and ASP.NET WebForms
- **Anti-Aliasing**: Smooth rendering with high-quality graphics
- **Coordinate Transformation**: Automatic transformation between geographic and screen coordinates

## Project Structure

```
MapVisualizer/
├── MapVisualizer/              # Core library
│   ├── MapHelper.cs            # Main API with DrawMap and GetCityAtPoint methods
│   ├── CityInfo.cs             # City data model
│   ├── CityAppearanceInfo.cs   # City appearance configuration
│   ├── geo.edmx                # Entity Framework model for geographic data
│   └── MapVisualizer.csproj    # Library project file
├── TestGeometryDraw/           # WinForms demonstration application
│   ├── Form1.cs                # Main form with map rendering and click handling
│   └── TestGeometryDraw.csproj # WinForms demo project file
├── TestGeometryDrawWebApplication/  # ASP.NET WebForms demonstration
│   ├── DrawMap.ashx            # HTTP handler for map rendering
│   ├── Default.aspx            # Web interface
│   └── TestGeometryDrawWebApplication.csproj  # Web demo project file
└── MapVisualizer.sln           # Solution file
```

## Prerequisites

- Visual Studio 2015 or later
- .NET Framework 4.5 or later
- SQL Server with geographic data support
- Entity Framework 6.x
- Database with city geometry data (using SqlGeometry types)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/gpavlovych/MapVisualizer.git
cd MapVisualizer
```

### Build the Solution

1. Open `MapVisualizer.sln` in Visual Studio
2. Restore NuGet packages (Visual Studio should do this automatically)
3. Build the solution (F6 or Build > Build Solution)

### Run the WinForms Demo

1. Set `TestGeometryDraw` as the startup project
2. Press F5 to run
3. The application will render a map of cities from your database
4. Click on any city to see its information

### Run the ASP.NET WebForms Demo

1. Set `TestGeometryDrawWebApplication` as the startup project
2. Press F5 to run in your browser
3. Navigate to the map page to see the rendered map
4. The map is served via an HTTP handler

## Usage

### WinForms Usage

```csharp
using System.Drawing;
using System.Windows.Forms;
using MapVisualizer;

public class MapForm : Form
{
    protected override void OnPaint(PaintEventArgs e)
    {
        // Define legend
        var legend = new Dictionary<Color, string>()
        {
            { ColorTranslator.FromHtml("#63BE7B"), ">95%" },
            { ColorTranslator.FromHtml("#BDD881"), "90-95%" },
            { ColorTranslator.FromHtml("#E9E583"), "60-90%" },
            { ColorTranslator.FromHtml("#FA8E72"), "30-60%" },
            { ColorTranslator.FromHtml("#E15151"), "<30%" }
        };

        // Draw the map
        using (var mapImage = MapHelper.DrawMap(
            new Size(800, 600),
            paddingLeft: 50,
            paddingRight: 50,
            paddingTop: 50,
            paddingBottom: 50,
            backgroundColor: Color.White,
            borderPen: new Pen(Color.Black, 1.0f),
            legendItems: legend,
            legendCaption: "Legend",
            legendFont: new Font(FontFamily.GenericMonospace, 8, FontStyle.Bold),
            legendFontColor: Color.Black,
            lineSpacing: 1.5f))
        {
            e.Graphics.DrawImageUnscaled(mapImage, 0, 0);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        // Get clicked city
        var city = MapHelper.GetCityAtPoint(
            new Size(ClientSize.Width, ClientSize.Height),
            paddingLeft: 50,
            paddingRight: 50,
            paddingTop: 50,
            paddingBottom: 50,
            point: new PointF(e.X, e.Y));

        if (city != null)
        {
            MessageBox.Show($"City: {city.CityName}\nColor: {city.CityColor}");
        }
    }
}
```

### ASP.NET WebForms Usage

```csharp
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using MapVisualizer;

public class DrawMapHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        // Parse dimensions from request
        int width = int.Parse(context.Request["width"] ?? "800");
        int height = int.Parse(context.Request["height"] ?? "600");

        // Define legend
        var legend = new Dictionary<Color, string>()
        {
            { ColorTranslator.FromHtml("#63BE7B"), ">95%" },
            { ColorTranslator.FromHtml("#BDD881"), "90-95%" }
        };

        // Draw and return map as PNG
        using (var mapImage = MapHelper.DrawMap(
            new Size(width, height),
            paddingLeft: 50,
            paddingRight: 50,
            paddingTop: 50,
            paddingBottom: 50,
            backgroundColor: Color.White,
            borderPen: new Pen(Color.Black, 1.0f),
            legendItems: legend,
            legendCaption: "Legend",
            legendFont: new Font(FontFamily.GenericMonospace, 8, FontStyle.Bold),
            legendFontColor: Color.Black,
            lineSpacing: 1.5f))
        {
            context.Response.ContentType = "image/png";
            mapImage.Save(context.Response.OutputStream, ImageFormat.Png);
        }
    }

    public bool IsReusable => false;
}
```

### Getting City Information from Coordinates

```csharp
// In your web handler
float x = float.Parse(context.Request["x"]);
float y = float.Parse(context.Request["y"]);

var city = MapHelper.GetCityAtPoint(
    new Size(width, height),
    paddingLeft: 50,
    paddingRight: 50,
    paddingTop: 50,
    paddingBottom: 50,
    point: new PointF(x, y));

if (city != null)
{
    context.Response.ContentType = "text/plain";
    context.Response.Write($"City: {city.CityName}");
}
else
{
    context.Response.StatusCode = 404;
}
```

## Technologies

- **C#** - Primary programming language
- **.NET Framework 4.5+** - Application framework
- **Entity Framework 6** - ORM for database access
- **System.Drawing** - Graphics rendering
- **System.Data.Entity.Spatial** - Geographic data types (DbGeometry)
- **ASP.NET WebForms** - Web application framework
- **Windows Forms** - Desktop application framework

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**gpavlovych** - [GitHub Profile](https://github.com/gpavlovych)
