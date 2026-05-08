using Corel.Interop.VGCore;
using CDR_AI_Assistant.Services.Interfaces;

namespace CDR_AI_Assistant.Services;

public class ToolboxService
{
    private readonly CDRHelper _cdrHelper;
    private readonly ConfigManager _config;

    public ToolboxService(CDRHelper cdrHelper, ConfigManager config)
    {
        _cdrHelper = cdrHelper;
        _config = config;
    }

    public void ConvertToCurves()
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        if (shapes.Count == 0)
        {
            _cdrHelper.ShowMessage("请先选中对象");
            return;
        }

        int convertedCount = 0;
        foreach (var shape in shapes)
        {
            try
            {
                if (shape.Type == cdrShapeType.cdrTextShape ||
                    shape.Type == cdrShapeType.cdrGroupShape)
                {
                    shape.ConvertToCurves();
                    convertedCount++;
                }
            }
            catch { }
        }

        _cdrHelper.ShowMessage($"已转换 {convertedCount} 个对象为曲线");
    }

    public void ExportSelectedAsPNG(string outputFolder)
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        if (shapes.Count == 0)
        {
            _cdrHelper.ShowMessage("请先选中对象");
            return;
        }

        var doc = _cdrHelper.GetActiveDocument();
        if (doc == null) return;

        var pageName = doc.Name ?? "untitled";
        int exported = 0;

        foreach (var shape in shapes)
        {
            try
            {
                var fileName = $"{pageName}_{exported++}.png";
                var fullPath = Path.Combine(outputFolder, fileName);
                shape.Export(fullPath, cdrExportFilterType.cdrPNG);
            }
            catch { }
        }

        _cdrHelper.ShowMessage($"已导出 {exported} 个文件到 {outputFolder}");
    }

    public void SmartAlign(string alignment)
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        if (shapes.Count < 2)
        {
            _cdrHelper.ShowMessage("请选中至少两个对象");
            return;
        }

        var page = _cdrHelper.GetActivePage();
        if (page == null) return;

        switch (alignment.ToLower())
        {
            case "center":
                var centerX = page.CenterX;
                var centerY = page.CenterY;
                foreach (var shape in shapes)
                {
                    shape.SetPosition(centerX - shape.Size.Width / 2,
                                     centerY - shape.Size.Height / 2);
                }
                break;

            case "left":
                var leftX = shapes.Min(s => s.LeftX);
                foreach (var shape in shapes)
                {
                    shape.SetPosition(leftX, shape.TopY);
                }
                break;

            case "right":
                var rightX = shapes.Max(s => s.RightX);
                foreach (var shape in shapes)
                {
                    shape.SetPosition(rightX - shape.Size.Width, shape.TopY);
                }
                break;

            default:
                _cdrHelper.ShowMessage($"未知对齐方式: {alignment}");
                break;
        }
    }
}