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
            DxfDocument doc = null;
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

            // Hilfsmethode zur Aktualisierung der Bounding-Box
            void UpdateBounds(double x, double y)
            {
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            foreach (var entity in doc.Entities)
            {
                switch (entity)
                {
                    case Line line:
                        totalLength += line.Length;
                        UpdateBounds(line.StartPoint.X, line.StartPoint.Y);
                        UpdateBounds(line.EndPoint.X, line.EndPoint.Y);
                        break;

                    case LwPolyline polyline:
                        for (int i = 0; i < polyline.Vertexes.Count - 1; i++)
                        {
                            var p1 = polyline.Vertexes[i].Position;
                            var p2 = polyline.Vertexes[i + 1].Position;
                            totalLength += p1.Distance(p2);
                            UpdateBounds(p1.X, p1.Y);
                            UpdateBounds(p2.X, p2.Y);
                        }
                        if (polyline.Vertexes.Any())
                        {
                            var last = polyline.Vertexes.Last().Position;
                            UpdateBounds(last.X, last.Y);
                        }
                        break;

                    case Circle circle:
                        totalLength += 2 * Math.PI * circle.Radius;
                        UpdateBounds(circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius);
                        UpdateBounds(circle.Center.X + circle.Radius, circle.Center.Y + circle.Radius);
                        break;

                    case Arc arc:
                        double startAngle = arc.StartAngle;
                        double endAngle = arc.EndAngle;
                        double angle = Math.Abs(endAngle - startAngle);
                        if (angle > 180)
                            angle = 360 - angle; // Korrektur, falls der Bogen über 0° geht.
                        double radAngle = angle * Math.PI / 180.0;
                        totalLength += radAngle * arc.Radius;

                        // Berechne Start- und Endpunkte des Bogens
                        double startX = arc.Center.X + arc.Radius * Math.Cos(startAngle * Math.PI / 180.0);
                        double startY = arc.Center.Y + arc.Radius * Math.Sin(startAngle * Math.PI / 180.0);
                        double endX = arc.Center.X + arc.Radius * Math.Cos(endAngle * Math.PI / 180.0);
                        double endY = arc.Center.Y + arc.Radius * Math.Sin(endAngle * Math.PI / 180.0);
                        UpdateBounds(startX, startY);
                        UpdateBounds(endX, endY);
                        break;

                    // Weitere Entitätstypen (z.B. Ellipse, Spline) können bei Bedarf ergänzt werden.
                    default:
                        break;
                }
            }

            double width = maxX - minX;
            double height = maxY - minY;

            Console.WriteLine($"Gesamte Schnittlänge: {totalLength:F2} Einheiten");
            Console.WriteLine($"Minimale Blechmaße: Breite = {width:F2} Einheiten, Höhe = {height:F2} Einheiten");
        }
    }
}
