using System;
using System.Linq;
using netDxf;
using netDxf.Entities;

namespace DXFLengthCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: DXFLengthCalculator <path_to_dxf_file>");
                return;
            }

            string filePath = args[0];
            DxfDocument doc;
            try
            {
                doc = DxfDocument.Load(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading DXF file: " + ex.Message);
                return;
            }

            double totalLength = 0.0;
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            // Methode zur Aktualisierung der Bounding-Box
            void UpdateBounds(double x, double y)
            {
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            // Linien verarbeiten:
            foreach (Line line in doc.Entities.Lines)
            {
                double length = Math.Sqrt(Math.Pow(line.EndPoint.X - line.StartPoint.X, 2) +
                                          Math.Pow(line.EndPoint.Y - line.StartPoint.Y, 2));
                totalLength += length;
                UpdateBounds(line.StartPoint.X, line.StartPoint.Y);
                UpdateBounds(line.EndPoint.X, line.EndPoint.Y);
            }

            // Polylinien verarbeiten über dynamischen Zugriff:
            foreach (Polyline polyline in doc.Entities.Polylines)
            {
                int count = polyline.Vertexes.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    var p1 = polyline.Vertexes[i].Position;
                    var p2 = polyline.Vertexes[i + 1].Position;
                    double segLength = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
                    totalLength += segLength;
                    UpdateBounds(p1.X, p1.Y);
                    UpdateBounds(p2.X, p2.Y);
                }
                
                // If the polyline is closed, add the segment from last to first point
                if (polyline.IsClosed && count > 1)
                {
                    var p1 = polyline.Vertexes[count - 1].Position;
                    var p2 = polyline.Vertexes[0].Position;
                    double segLength = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
                    totalLength += segLength;
                }
            }
        var p2 = polyline.Vertexes[0].Position;
        double segLength = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        totalLength += segLength;
    }
}
        var p2 = polyline.Vertexes[0].Position;
        double segLength = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        totalLength += segLength;
    }
}
            // Kreise verarbeiten:
            foreach (Circle circle in doc.Entities.Circles)
            {
                totalLength += 2 * Math.PI * circle.Radius;
                UpdateBounds(circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius);
                UpdateBounds(circle.Center.X + circle.Radius, circle.Center.Y + circle.Radius);
            }

            // Bögen (Arcs) verarbeiten:
            foreach (Arc arc in doc.Entities.Arcs)
            {
                double startAngle = arc.StartAngle;
                double endAngle = arc.EndAngle;
                double angle = Math.Abs(endAngle - startAngle);
                if (angle > 180)
                    angle = 360 - angle; // Falls der Bogen über 0° hinausgeht.
                double radAngle = angle * Math.PI / 180.0;
                totalLength += radAngle * arc.Radius;

                double startX = arc.Center.X + arc.Radius * Math.Cos(startAngle * Math.PI / 180.0);
                double startY = arc.Center.Y + arc.Radius * Math.Sin(startAngle * Math.PI / 180.0);
                double endX = arc.Center.X + arc.Radius * Math.Cos(endAngle * Math.PI / 180.0);
                double endY = arc.Center.Y + arc.Radius * Math.Sin(endAngle * Math.PI / 180.0);
                UpdateBounds(startX, startY);
                UpdateBounds(endX, endY);
            }

            double width = maxX - minX;
            double height = maxY - minY;

            Console.WriteLine($"Total cut length: {totalLength:F2} units");
            Console.WriteLine($"Minimum sheet dimensions: width = {width:F2} units, height = {height:F2} units");
        }
    }
}

