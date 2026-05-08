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
}