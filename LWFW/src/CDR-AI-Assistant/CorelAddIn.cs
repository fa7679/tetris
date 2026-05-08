using System.Runtime.InteropServices;
using Corel.Interop.VGCore;

namespace CDR_AI_Assistant;

[ComVisible(true)]
[Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
public class CorelAddIn : IGmsApplication
{
    private CDRHelper? _cdrHelper;
    private MainWindow? _mainWindow;

    public void OnLoad()
    {
        try
        {
            _cdrHelper = new CDRHelper(Application);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow = new MainWindow(_cdrHelper);
                _mainWindow.Show();
            });

            _cdrHelper.ShowMessage("CDR AI 助手已加载！");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"插件加载失败: {ex.Message}",
                "错误", System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    public void OnUnload()
    {
        _mainWindow?.Close();
        _mainWindow = null;
        _cdrHelper = null;
    }

    public void OnGmsRun(string gmsName, int lBand, int hBand)
    {
    }

    public int GetCaps(int caps)
    {
        return 0;
    }
}