using Corel.Interop.VGCore;
using CDR_AI_Assistant.Services.Interfaces;

namespace CDR_AI_Assistant.Services;

public class ToolboxService
{
    private readonly CDRHelper _cdrHelper;
    private readonly ConfigManager _config;
    private readonly AlignmentService _alignmentService;
    private readonly ExportService _exportService;

    public ToolboxService(CDRHelper cdrHelper, ConfigManager config)
    {
        _cdrHelper = cdrHelper;
        _config = config;
        _alignmentService = new AlignmentService(cdrHelper);
        _exportService = new ExportService(cdrHelper);
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
                if (shape.Type == cdrShapeType.cdrTextShape)
                {
                    shape.ConvertToCurves();
                    convertedCount++;
                }
                else if (shape.Type == cdrShapeType.cdrGroupShape)
                {
                    _cdrHelper.ConvertGroupRecursively(shape);
                    convertedCount++;
                }
            }
            catch { }
        }

        _cdrHelper.ShowMessage($"已转换 {convertedCount} 个对象为曲线");
    }

    public async Task ExportSelectedAsPNGAsync(string outputFolder, Action<int, int>? progressCallback = null)
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        if (shapes.Count == 0)
        {
            _cdrHelper.ShowMessage("请先选中对象");
            return;
        }

        await _exportService.ExportShapesAsPNGAsync(shapes, outputFolder, progressCallback);
        _cdrHelper.ShowMessage($"已导出 {shapes.Count} 个文件到 {outputFolder}");
    }

    public async Task ExportSelectedAsJPGAsync(string outputFolder, int quality = 90, Action<int, int>? progressCallback = null)
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        if (shapes.Count == 0)
        {
            _cdrHelper.ShowMessage("请先选中对象");
            return;
        }

        await _exportService.ExportShapesAsJPGAsync(shapes, outputFolder, quality, progressCallback);
        _cdrHelper.ShowMessage($"已导出 {shapes.Count} 个文件到 {outputFolder}");
    }

    public void SmartAlign(string alignment)
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        if (shapes.Count < 2)
        {
            _cdrHelper.ShowMessage("请选中至少两个对象");
            return;
        }

        switch (alignment.ToLower())
        {
            case "center":
                _alignmentService.AlignToPageCenter(shapes);
                _cdrHelper.ShowMessage("已居中对齐");
                break;

            case "left":
                _alignmentService.AlignLeft(shapes);
                _cdrHelper.ShowMessage("已左对齐");
                break;

            case "right":
                _alignmentService.AlignRight(shapes);
                _cdrHelper.ShowMessage("已右对齐");
                break;

            case "top":
                _alignmentService.AlignTop(shapes);
                _cdrHelper.ShowMessage("已上对齐");
                break;

            case "bottom":
                _alignmentService.AlignBottom(shapes);
                _cdrHelper.ShowMessage("已下对齐");
                break;

            case "hcenter":
                _alignmentService.AlignCenter(shapes, horizontal: true);
                _cdrHelper.ShowMessage("已水平居中对齐");
                break;

            case "vcenter":
                _alignmentService.AlignCenter(shapes, horizontal: false);
                _cdrHelper.ShowMessage("已垂直居中对齐");
                break;

            default:
                _cdrHelper.ShowMessage($"未知对齐方式: {alignment}");
                break;
        }
    }

    public void AlignToPageCenter()
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        if (!shapes.Any())
        {
            _cdrHelper.ShowMessage("请先选中对象");
            return;
        }

        _alignmentService.AlignToPageCenter(shapes);
        _cdrHelper.ShowMessage("已对齐到页面中心");
    }
}