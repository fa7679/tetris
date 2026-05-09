using Corel.Interop.VGCore;

namespace CDR_AI_Assistant.Services;

public class ExportService
{
    private readonly CDRHelper _cdrHelper;

    public ExportService(CDRHelper cdrHelper)
    {
        _cdrHelper = cdrHelper;
    }

    public async Task ExportShapesAsPNGAsync(
        IEnumerable<Shape> shapes,
        string outputFolder,
        Action<int, int>? progressCallback = null)
    {
        var shapeList = shapes.ToList();
        var total = shapeList.Count;
        int current = 0;

        var doc = _cdrHelper.GetActiveDocument();
        var pageName = doc?.Name ?? "untitled";

        if (!System.IO.Directory.Exists(outputFolder))
            System.IO.Directory.CreateDirectory(outputFolder);

        foreach (var shape in shapeList)
        {
            current++;
            progressCallback?.Invoke(current, total);

            var timestamp = DateTime.Now.ToString("HHmmss");
            var fileName = $"{pageName}_{shape.Id}_{timestamp}.png";
            var fullPath = System.IO.Path.Combine(outputFolder, fileName);

            try
            {
                shape.Export(fullPath, Corel.Interop.VGCore.cdrExportFilterType.cdrPNG);
            }
            catch { }
        }
    }

    public async Task ExportShapesAsJPGAsync(
        IEnumerable<Shape> shapes,
        string outputFolder,
        int quality = 90,
        Action<int, int>? progressCallback = null)
    {
        var shapeList = shapes.ToList();
        var total = shapeList.Count;
        int current = 0;

        var doc = _cdrHelper.GetActiveDocument();
        var pageName = doc?.Name ?? "untitled";

        if (!System.IO.Directory.Exists(outputFolder))
            System.IO.Directory.CreateDirectory(outputFolder);

        foreach (var shape in shapeList)
        {
            current++;
            progressCallback?.Invoke(current, total);

            var timestamp = DateTime.Now.ToString("HHmmss");
            var fileName = $"{pageName}_{shape.Id}_{timestamp}.jpg";
            var fullPath = System.IO.Path.Combine(outputFolder, fileName);

            try
            {
                shape.Export(fullPath, Corel.Interop.VGCore.cdrExportFilterType.cdrJPEG);
            }
            catch { }
        }
    }
}