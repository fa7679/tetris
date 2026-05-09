using Corel.Interop.VGCore;
using CDR_AI_Assistant.Services.Interfaces;

namespace CDR_AI_Assistant.Services;

public class AlignmentService : IAlignmentService
{
    private readonly CDRHelper _cdrHelper;

    public AlignmentService(CDRHelper cdrHelper)
    {
        _cdrHelper = cdrHelper;
    }

    public void AlignCenter(IEnumerable<Shape> shapes, bool horizontal)
    {
        var shapeList = shapes.ToList();
        if (shapeList.Count < 2) return;

        var page = _cdrHelper.GetActivePage();
        if (page == null) return;

        if (horizontal)
        {
            var centerX = page.CenterX;
            foreach (var shape in shapeList)
            {
                shape.SetPosition(centerX - shape.Size.Width / 2, shape.TopY);
            }
        }
        else
        {
            var centerY = page.CenterY;
            foreach (var shape in shapeList)
            {
                shape.SetPosition(shape.LeftX, centerY - shape.Size.Height / 2);
            }
        }
    }

    public void AlignLeft(IEnumerable<Shape> shapes)
    {
        var shapeList = shapes.ToList();
        if (shapeList.Count < 2) return;

        var minLeftX = shapeList.Min(s => s.LeftX);
        foreach (var shape in shapeList)
        {
            shape.SetPosition(minLeftX, shape.TopY);
        }
    }

    public void AlignRight(IEnumerable<Shape> shapes)
    {
        var shapeList = shapes.ToList();
        if (shapeList.Count < 2) return;

        var maxRightX = shapeList.Max(s => s.LeftX + s.Size.Width);
        foreach (var shape in shapeList)
        {
            shape.SetPosition(maxRightX - shape.Size.Width, shape.TopY);
        }
    }

    public void AlignTop(IEnumerable<Shape> shapes)
    {
        var shapeList = shapes.ToList();
        if (shapeList.Count < 2) return;

        var minTopY = shapeList.Min(s => s.TopY);
        foreach (var shape in shapeList)
        {
            shape.SetPosition(shape.LeftX, minTopY);
        }
    }

    public void AlignBottom(IEnumerable<Shape> shapes)
    {
        var shapeList = shapes.ToList();
        if (shapeList.Count < 2) return;

        var maxBottomY = shapeList.Max(s => s.TopY + s.Size.Height);
        foreach (var shape in shapeList)
        {
            shape.SetPosition(shape.LeftX, maxBottomY - shape.Size.Height);
        }
    }

    public void AlignToPageCenter(IEnumerable<Shape> shapes)
    {
        var shapeList = shapes.ToList();
        if (!shapeList.Any()) return;

        var pageCenter = _cdrHelper.GetPageCenter();
        foreach (var shape in shapeList)
        {
            shape.SetPosition(
                pageCenter.centerX - shape.Size.Width / 2,
                pageCenter.centerY - shape.Size.Height / 2);
        }
    }
}