using System;
using System.Linq;
using Rhino.Geometry;

namespace NL2Geo.Adapters;

public interface IRhinoCommandAdapter
{
    bool HasSelection();
    void WriteLine(string message);
    Guid? AddBox(double width, double height, double depth);
    Guid? AddCylinder(double radius, double height);
    Guid? AddSphere(double radius);
    Guid? AddCone(double radius, double height);
    Guid? AddTorus(double majorRadius, double minorRadius);
    Guid? AddPyramid(double baseWidth, double baseDepth, double height);
    Guid? AddEllipsoid(double radiusX, double radiusY, double radiusZ);
    Guid? AddCircle(double radius);
    Guid? AddRectangle(double width, double height);
    Guid? AddLine(double x1, double y1, double z1, double x2, double y2, double z2);
    Guid? AddPoint(double x, double y, double z);
    void MoveActive(double x, double y, double z);
    void RotateActive(double degrees, string axis);
    void ScaleActive(double factor);
    void ArrayLinearActive(int count, double spacing, string axis);
    void ArrayGridActive(int countX, int countY, double spacing);
    void ArrayPolarActive(int count, double radius);
    void Redraw();
}

public sealed class RhinoRuntimeCommandAdapter : IRhinoCommandAdapter
{
    private readonly Rhino.RhinoDoc _doc;
    private readonly HashSet<Guid> _activeObjectIds = new();

    public RhinoRuntimeCommandAdapter(Rhino.RhinoDoc doc)
    {
        _doc = doc;
    }

    public bool HasSelection() => _doc.Objects.GetSelectedObjects(false, false).Any();

    public void WriteLine(string message)
    {
        Rhino.RhinoApp.WriteLine(message);
    }

    public Guid? AddBox(double width, double height, double depth)
    {
        var half = new Vector3d(width / 2.0, height / 2.0, depth / 2.0);
        var min = new Point3d(-half.X, -half.Y, 0);
        var max = new Point3d(half.X, half.Y, depth);
        var box = new Box(Plane.WorldXY, new BoundingBox(min, max));
        var id = _doc.Objects.AddBox(box);
        return TrackNew(id);
    }

    public Guid? AddCylinder(double radius, double height)
    {
        var circle = new Circle(Plane.WorldXY, radius);
        var cyl = new Cylinder(circle, height);
        var brep = cyl.ToBrep(true, true);
        var id = _doc.Objects.AddBrep(brep);
        return TrackNew(id);
    }

    public Guid? AddSphere(double radius)
    {
        var sphere = new Sphere(Point3d.Origin, radius);
        var id = _doc.Objects.AddSphere(sphere);
        return TrackNew(id);
    }

    public Guid? AddCone(double radius, double height)
    {
        var basePlane = new Plane(new Point3d(0, 0, height), Vector3d.ZAxis);
        var cone = new Cone(basePlane, -height, radius);
        var brep = cone.ToBrep(true);
        var id = _doc.Objects.AddBrep(brep);
        return TrackNew(id);
    }

    public Guid? AddTorus(double majorRadius, double minorRadius)
    {
        var torus = new Torus(Plane.WorldXY, majorRadius, minorRadius);
        var revSurface = torus.ToRevSurface();
        var brep = Brep.CreateFromRevSurface(revSurface, true, true);
        var id = _doc.Objects.AddBrep(brep);
        return TrackNew(id);
    }

    public Guid? AddPyramid(double baseWidth, double baseDepth, double height)
    {
        var hw = baseWidth / 2.0;
        var hd = baseDepth / 2.0;
        var p0 = new Point3d(-hw, -hd, 0);
        var p1 = new Point3d(hw, -hd, 0);
        var p2 = new Point3d(hw, hd, 0);
        var p3 = new Point3d(-hw, hd, 0);
        var apex = new Point3d(0, 0, height);
        var faces = new[]
        {
            Brep.CreateFromCornerPoints(p0, p1, p2, p3, 0.001),
            Brep.CreateFromCornerPoints(p0, p1, apex, 0.001),
            Brep.CreateFromCornerPoints(p1, p2, apex, 0.001),
            Brep.CreateFromCornerPoints(p2, p3, apex, 0.001),
            Brep.CreateFromCornerPoints(p3, p0, apex, 0.001)
        };
        var joined = Brep.JoinBreps(faces, 0.001) ?? faces;
        var id = Guid.Empty;
        foreach (var brep in joined)
        {
            id = _doc.Objects.AddBrep(brep);
        }
        return TrackNew(id);
    }

    public Guid? AddEllipsoid(double radiusX, double radiusY, double radiusZ)
    {
        var sphere = new Sphere(Point3d.Origin, 1.0);
        var brep = sphere.ToBrep();
        var scale = Transform.Scale(Plane.WorldXY, radiusX, radiusY, radiusZ);
        brep.Transform(scale);
        var id = _doc.Objects.AddBrep(brep);
        return TrackNew(id);
    }

    public Guid? AddCircle(double radius)
    {
        var circle = new Circle(Plane.WorldXY, radius);
        var id = _doc.Objects.AddCircle(circle);
        return TrackNew(id);
    }

    public Guid? AddRectangle(double width, double height)
    {
        var hw = width / 2.0;
        var hh = height / 2.0;
        var rect = new Rectangle3d(Plane.WorldXY, new Point3d(-hw, -hh, 0), new Point3d(hw, hh, 0));
        var id = _doc.Objects.AddPolyline(rect.ToPolyline());
        return TrackNew(id);
    }

    public Guid? AddLine(double x1, double y1, double z1, double x2, double y2, double z2)
    {
        var line = new Line(new Point3d(x1, y1, z1), new Point3d(x2, y2, z2));
        var id = _doc.Objects.AddLine(line);
        return TrackNew(id);
    }

    public Guid? AddPoint(double x, double y, double z)
    {
        var id = _doc.Objects.AddPoint(new Point3d(x, y, z));
        return TrackNew(id);
    }

    public void MoveActive(double x, double y, double z)
    {
        var ids = ResolveActiveObjectIds();
        if (ids.Count == 0) return;
        var xform = Transform.Translation(x, y, z);
        ApplyTransform(ids, xform);
    }

    public void RotateActive(double degrees, string axis)
    {
        var ids = ResolveActiveObjectIds();
        if (ids.Count == 0) return;
        var radians = Rhino.RhinoMath.ToRadians(degrees);
        var axisVector = axis.ToLowerInvariant() switch
        {
            "x" => Vector3d.XAxis,
            "y" => Vector3d.YAxis,
            _ => Vector3d.ZAxis
        };
        var center = GetSelectionCenter(ids);
        var xform = Transform.Rotation(radians, axisVector, center);
        ApplyTransform(ids, xform);
    }

    public void ScaleActive(double factor)
    {
        var ids = ResolveActiveObjectIds();
        if (ids.Count == 0) return;
        var center = GetSelectionCenter(ids);
        var xform = Transform.Scale(center, factor);
        ApplyTransform(ids, xform);
    }

    public void ArrayLinearActive(int count, double spacing, string axis)
    {
        var ids = ResolveActiveObjectIds();
        if (ids.Count == 0 || count <= 1) return;
        var dir = axis.ToLowerInvariant() switch
        {
            "y" => Vector3d.YAxis,
            "z" => Vector3d.ZAxis,
            _ => Vector3d.XAxis
        };
        var newIds = new List<Guid>();
        for (var i = 1; i < count; i++)
        {
            var xform = Transform.Translation(dir * (spacing * i));
            newIds.AddRange(DuplicateWithTransform(ids, xform));
        }
        SetActive(ids.Concat(newIds));
    }

    public void ArrayGridActive(int countX, int countY, double spacing)
    {
        var ids = ResolveActiveObjectIds();
        if (ids.Count == 0 || countX <= 0 || countY <= 0) return;
        var newIds = new List<Guid>();
        for (var ix = 0; ix < countX; ix++)
        {
            for (var iy = 0; iy < countY; iy++)
            {
                if (ix == 0 && iy == 0) continue;
                var xform = Transform.Translation(ix * spacing, iy * spacing, 0);
                newIds.AddRange(DuplicateWithTransform(ids, xform));
            }
        }
        SetActive(ids.Concat(newIds));
    }

    public void ArrayPolarActive(int count, double radius)
    {
        var ids = ResolveActiveObjectIds();
        if (ids.Count == 0 || count <= 1) return;
        var center = GetSelectionCenter(ids);
        var newIds = new List<Guid>();
        for (var i = 1; i < count; i++)
        {
            var a = 2.0 * Math.PI * i / count;
            var move = Transform.Translation(
                center.X + Math.Cos(a) * radius - center.X,
                center.Y + Math.Sin(a) * radius - center.Y,
                0
            );
            newIds.AddRange(DuplicateWithTransform(ids, move));
        }
        SetActive(ids.Concat(newIds));
    }

    private Guid? TrackNew(Guid id)
    {
        if (id == Guid.Empty) return null;
        SetActive(new[] { id });
        return id;
    }

    private void SetActive(IEnumerable<Guid> ids)
    {
        _activeObjectIds.Clear();
        foreach (var id in ids.Where(id => id != Guid.Empty))
        {
            _activeObjectIds.Add(id);
        }
    }

    private List<Guid> ResolveActiveObjectIds()
    {
        var selected = _doc.Objects.GetSelectedObjects(false, false)
            .Select(o => o.Id)
            .ToList();
        if (selected.Count > 0) return selected;
        return _activeObjectIds
            .Where(id => _doc.Objects.FindId(id) is not null)
            .ToList();
    }

    private void ApplyTransform(IEnumerable<Guid> ids, Transform xform)
    {
        foreach (var id in ids)
        {
            _doc.Objects.Transform(id, xform, false);
        }
    }

    private IEnumerable<Guid> DuplicateWithTransform(IEnumerable<Guid> ids, Transform xform)
    {
        foreach (var id in ids)
        {
            var rhObj = _doc.Objects.FindId(id);
            if (rhObj is null) continue;
            var geometry = rhObj.Geometry.Duplicate();
            if (geometry is null) continue;
            geometry.Transform(xform);
            var nid = _doc.Objects.Add(geometry);
            if (nid != Guid.Empty) yield return nid;
        }
    }

    private Point3d GetSelectionCenter(IEnumerable<Guid> ids)
    {
        BoundingBox? bbox = null;
        foreach (var id in ids)
        {
            var rhObj = _doc.Objects.FindId(id);
            if (rhObj is null) continue;
            var gb = rhObj.Geometry.GetBoundingBox(true);
            bbox = bbox is null ? gb : BoundingBox.Union(bbox.Value, gb);
        }
        return bbox?.Center ?? Point3d.Origin;
    }

    public void Redraw() => _doc.Views.Redraw();
}

public sealed class RhinoCommandAdapter : IRhinoCommandAdapter
{
    public bool SelectionPresent { get; set; }
    public List<string> OutputLog { get; } = new();
    public List<string> AddedGeometry { get; } = new();

    public bool HasSelection() => SelectionPresent;

    public void WriteLine(string message)
    {
        OutputLog.Add(message);
    }

    public Guid? AddBox(double width, double height, double depth)
    {
        AddedGeometry.Add($"box:{width}x{height}x{depth}");
        return Guid.NewGuid();
    }

    public Guid? AddCylinder(double radius, double height)
    {
        AddedGeometry.Add($"cylinder:r={radius},h={height}");
        return Guid.NewGuid();
    }

    public Guid? AddSphere(double radius)
    {
        AddedGeometry.Add($"sphere:r={radius}");
        return Guid.NewGuid();
    }

    public Guid? AddCone(double radius, double height)
    {
        AddedGeometry.Add($"cone:r={radius},h={height}");
        return Guid.NewGuid();
    }

    public Guid? AddTorus(double majorRadius, double minorRadius)
    {
        AddedGeometry.Add($"torus:R={majorRadius},r={minorRadius}");
        return Guid.NewGuid();
    }

    public Guid? AddPyramid(double baseWidth, double baseDepth, double height)
    {
        AddedGeometry.Add($"pyramid:{baseWidth}x{baseDepth}x{height}");
        return Guid.NewGuid();
    }

    public Guid? AddEllipsoid(double radiusX, double radiusY, double radiusZ)
    {
        AddedGeometry.Add($"ellipsoid:{radiusX}x{radiusY}x{radiusZ}");
        return Guid.NewGuid();
    }

    public Guid? AddCircle(double radius)
    {
        AddedGeometry.Add($"circle:r={radius}");
        return Guid.NewGuid();
    }

    public Guid? AddRectangle(double width, double height)
    {
        AddedGeometry.Add($"rectangle:{width}x{height}");
        return Guid.NewGuid();
    }

    public Guid? AddLine(double x1, double y1, double z1, double x2, double y2, double z2)
    {
        AddedGeometry.Add($"line:({x1},{y1},{z1})->({x2},{y2},{z2})");
        return Guid.NewGuid();
    }

    public Guid? AddPoint(double x, double y, double z)
    {
        AddedGeometry.Add($"point:({x},{y},{z})");
        return Guid.NewGuid();
    }

    public void MoveActive(double x, double y, double z) => OutputLog.Add($"move:{x},{y},{z}");

    public void RotateActive(double degrees, string axis) => OutputLog.Add($"rotate:{degrees},{axis}");

    public void ScaleActive(double factor) => OutputLog.Add($"scale:{factor}");

    public void ArrayLinearActive(int count, double spacing, string axis) => OutputLog.Add($"array_linear:{count},{spacing},{axis}");

    public void ArrayGridActive(int countX, int countY, double spacing) => OutputLog.Add($"array_grid:{countX},{countY},{spacing}");

    public void ArrayPolarActive(int count, double radius) => OutputLog.Add($"array_polar:{count},{radius}");

    public void Redraw() { }
}
