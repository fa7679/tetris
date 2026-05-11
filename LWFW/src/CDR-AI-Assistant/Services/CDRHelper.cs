using System.Runtime.InteropServices;
using Corel.Interop.VGCore;

namespace CDR_AI_Assistant.Services;

public class CDRHelper
{
    private static CDRHelper? _instance;
    private static Corel.Interop.VGCore.Application? _cachedApp;
    private readonly Corel.Interop.VGCore.Application _cdrApp;

    public CDRHelper(Corel.Interop.VGCore.Application app)
    {
        _cdrApp = app ?? throw new ArgumentNullException(nameof(app));
    }

    public static CDRHelper GetApplication()
    {
        if (_instance != null) return _instance;

        if (_cachedApp != null)
        {
            _instance = new CDRHelper(_cachedApp);
            return _instance;
        }

        var unk = CoGetActiveObject("CorelDRAW.Application");
        if (unk != null)
        {
            _cachedApp = (Corel.Interop.VGCore.Application)unk;
            _instance = new CDRHelper(_cachedApp);
            return _instance;
        }

        throw new InvalidOperationException("Cannot get CorelDRAW Application instance");
    }

    [DllImport("ole32.dll")]
    private static extern int CoGetActiveObject(
        [MarshalAs(UnmanagedType.LPWStr)] string clsid,
        IntPtr pUnk,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    private static object? CoGetActiveObject(string clsid)
    {
        var hr = CoGetActiveObject(clsid, IntPtr.Zero, out var obj);
        if (hr == 0 && obj != null)
            return obj;
        return null;
    }

    public Corel.Interop.VGCore.Application App => _cdrApp;

    public Document? GetActiveDocument() => _cdrApp.ActiveDocument;

    public Page? GetActivePage()
    {
        var doc = GetActiveDocument();
        return doc?.ActivePage;
    }

    public dynamic GetSelection()
    {
        return _cdrApp.ActiveSelection;
    }
}