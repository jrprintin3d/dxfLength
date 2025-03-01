import sys
import ezdxf
import math

def update_bounds(bounds, x, y):
    bounds['min_x'] = min(bounds['min_x'], x)
    bounds['max_x'] = max(bounds['max_x'], x)
    bounds['min_y'] = min(bounds['min_y'], y)
    bounds['max_y'] = max(bounds['max_y'], y)

def in_arc(test_angle_deg, start_angle_deg, end_angle_deg):
    # Normalisierung der Winkel
    start = start_angle_deg % 360
    end = end_angle_deg % 360
    test = test_angle_deg % 360
    if start < end:
        return start <= test <= end
    else:
        return test >= start or test <= end

# Initialisierung der Bounding-Box mit Extremwerten
bounds = {
    'min_x': float('inf'),
    'max_x': float('-inf'),
    'min_y': float('inf'),
    'max_y': float('-inf')
}

dxf_file = sys.argv[1]
doc = ezdxf.readfile(dxf_file)
msp = doc.modelspace()

total_length = 0.0

# Verarbeitung von LINE-Objekten
for line in msp.query('LINE'):
    if line.dxf.linetype not in ['Hidden', 'Dashed']:  # Beispielhafte Filterung
        start = line.dxf.start
        end = line.dxf.end
        total_length += math.dist(start, end)
        update_bounds(bounds, start[0], start[1])
        update_bounds(bounds, end[0], end[1])

# Verarbeitung von LWPOLYLINE-Objekten
for poly in msp.query('LWPOLYLINE'):
    if poly.dxf.linetype not in ['Hidden', 'Dashed']:
        pts = poly.get_points('xy')
        if len(pts) > 1:
            total_length += sum(math.dist(pts[i], pts[i+1]) for i in range(len(pts)-1))
        for pt in pts:
            update_bounds(bounds, pt[0], pt[1])

# Verarbeitung von CIRCLE-Objekten (Umfang & Bounding-Box)
for circle in msp.query('CIRCLE'):
    if circle.dxf.linetype not in ['Hidden', 'Dashed']:
        center = circle.dxf.center
        r = circle.dxf.radius
        total_length += 2 * math.pi * r
        # Kreis vollständig in die Bounding-Box einbeziehen
        update_bounds(bounds, center[0] - r, center[1] - r)
        update_bounds(bounds, center[0] + r, center[1] + r)

# Verarbeitung von ARC-Objekten
for arc in msp.query('ARC'):
    if arc.dxf.linetype not in ['Hidden', 'Dashed']:
        center = arc.dxf.center
        r = arc.dxf.radius
        angle = abs(arc.dxf.end_angle - arc.dxf.start_angle)
        total_length += (angle / 360.0) * (2 * math.pi * r)
        # Berechnung der Endpunkte
        start_angle_rad = math.radians(arc.dxf.start_angle)
        end_angle_rad = math.radians(arc.dxf.end_angle)
        start_point = (center[0] + r * math.cos(start_angle_rad), center[1] + r * math.sin(start_angle_rad))
        end_point = (center[0] + r * math.cos(end_angle_rad), center[1] + r * math.sin(end_angle_rad))
        update_bounds(bounds, start_point[0], start_point[1])
        update_bounds(bounds, end_point[0], end_point[1])
        # Prüfung auf kardinale Richtungen (0°, 90°, 180°, 270°) falls im Bogen enthalten
        for test_angle in [0, 90, 180, 270]:
            if in_arc(test_angle, arc.dxf.start_angle, arc.dxf.end_angle):
                test_rad = math.radians(test_angle)
                test_point = (center[0] + r * math.cos(test_rad), center[1] + r * math.sin(test_rad))
                update_bounds(bounds, test_point[0], test_point[1])

# Weitere Entitätstypen (z.B. ELLIPSE, SPLINE) können analog behandelt werden

# Berechnung der minimalen Blechmaße
width = bounds['max_x'] - bounds['min_x']
height = bounds['max_y'] - bounds['min_y']

# Ausgabe der Ergebnisse
print(f"Gesamte Schnittlänge: {total_length:.2f} Einheiten")
print(f"Minimale Blechmaße: Breite = {width:.2f} Einheiten, Höhe = {height:.2f} Einheiten")
