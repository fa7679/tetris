using System.Runtime.InteropServices;
using Corel.Interop.VGCore;

namespace CDR_AI_Assistant.Services;

public class CDRHelper
{
    private readonly Application _cdrApp;

    public CDRHelper(Application app)
    {
        _cdrApp = app ?? throw new ArgumentNullException(nameof(app));
    }

    public Application App => _cdrApp;

    public Document? GetActiveDocument() => _cdrApp.ActiveDocument;

    public Page? GetActivePage()
    {
        var doc = GetActiveDocument();
        return doc?.ActivePage;
    }

    public IEnumerable<Shape> GetSelectedShapes()
    {
        var selection = _cdrApp.ActiveSelection;
        if (selection == null || selection.Shapes.Count == 0)
            return Enumerable.Empty<Shape>();

        var shapes = new List<Shape>();
        foreach (Shape shape in selection.Shapes)
        {
            shapes.Add(shape);
        }
        return shapes;
    }

    public Shape? CreateRectangle(double width = 1000, double height = 500)
    {
        var page = GetActivePage();
        if (page == null) return null;

        return page.CreateRectangle2(
            page.Size.Width / 2 - width / 2,
            page.Size.Height / 2 - height / 2,
            width, height);
    }

    public Shape? ImportImage(string filePath)
    {
        var page = GetActivePage();
        return page?.Import(filePath);
    }

    public void ShowMessage(string message, string title = "CDR AI 助手")
    {
        System.Windows.MessageBox.Show(message, title,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "错误")
    {
        System.Windows.MessageBox.Show(message, title,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error);
    }

    public static void ReleaseComObject(object? obj)
    {
        if (obj != null && Marshal.IsComObject(obj))
            Marshal.ReleaseComObject(obj);
    }

    public string GetSelectedTextContent()
    {
        var shapes = GetSelectedShapes();
        var textBuilder = new System.Text.StringBuilder();

        foreach (var shape in shapes)
        {
            textBuilder.AppendLine(GetTextFromShape(shape));
        }

        return textBuilder.ToString().TrimEnd();
    }

    private string GetTextFromShape(Shape shape)
    {
        if (shape.Type == Corel.Interop.VGCore.cdrShapeType.cdrTextShape)
        {
            return shape.Text?.Story?.Text ?? string.Empty;
        }
        else if (shape.Type == Corel.Interop.VGCore.cdrShapeType.cdrGroupShape)
        {
            var result = new System.Text.StringBuilder();
            foreach (Shape child in shape.Shapes)
            {
                result.AppendLine(GetTextFromShape(child));
            }
            return result.ToString();
        }
        return string.Empty;
    }

    public (double centerX, double centerY) GetPageCenter()
    {
        var page = GetActivePage();
        if (page == null) return (0, 0);
        return (page.CenterX, page.CenterY);
    }

    public Shape? CreateArtisticTextAtPosition(double x, double y, string text,
        Corel.Interop.VGCore.cdrTextAlignment alignment = Corel.Interop.VGCore.cdrTextAlignment.cdrCenterAlignment)
    {
        var page = GetActivePage();
        if (page == null) return null;

        return page.CreateArtisticText(x, y, text, alignment);
    }

    public void ConvertGroupRecursively(Shape groupShape)
    {
        if (groupShape.Type != Corel.Interop.VGCore.cdrShapeType.cdrGroupShape)
            return;

        var shapesToConvert = new List<Shape>();
        CollectShapesToConvert(groupShape, shapesToConvert);

        foreach (var shape in shapesToConvert)
        {
            try
            {
                shape.ConvertToCurves();
            }
            catch { }
        }
    }

    private void CollectShapesToConvert(Shape parent, List<Shape> collection)
    {
        foreach (Shape child in parent.Shapes)
        {
            if (child.Type == Corel.Interop.VGCore.cdrShapeType.cdrGroupShape)
            {
                CollectShapesToConvert(child, collection);
            }
            else
            {
                collection.Add(child);
            }
        }
    }

    public (double width, double height) GetPageSize()
    {
        var page = GetActivePage();
        if (page == null) return (0, 0);
        return (page.Size.Width, page.Size.Height);
    }
}