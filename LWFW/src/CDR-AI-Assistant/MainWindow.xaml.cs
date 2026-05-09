using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CDR_AI_Assistant.Models;
using CDR_AI_Assistant.Services;

namespace CDR_AI_Assistant;

public partial class MainWindow : Window
{
    private readonly CDRHelper _cdrHelper;
    private readonly ConfigManager _configManager;
    private readonly AIServiceFactory _aiServiceFactory;
    private readonly TranslationServiceFactory _translationServiceFactory;
    private readonly HttpClient _httpClient;
    private readonly ToolboxService _toolboxService;
    private readonly TextSelectionService _textSelectionService;

    public MainWindow(CDRHelper cdrHelper)
    {
        InitializeComponent();

        _cdrHelper = cdrHelper;
        _configManager = new ConfigManager();
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        _aiServiceFactory = new AIServiceFactory(_configManager, _httpClient);
        _translationServiceFactory = new TranslationServiceFactory(_httpClient, _configManager);
        _toolboxService = new ToolboxService(_cdrHelper, _configManager);
        _textSelectionService = new TextSelectionService(cdrHelper);

        LoadConfig();
    }

    private void LoadConfig()
    {
        try
        {
            var config = _configManager.Load();
            BaseURLInput.Text = config.BaseURL;
            ModelNameInput.Text = config.ModelName;
            TimeoutInput.Text = config.TimeoutSeconds.ToString();
            DefaultImageSizeInput.Text = config.DefaultImageSize;
            DefaultExportFormatSelector.SelectedIndex = config.DefaultExportFormat == "JPG" ? 1 : 0;

            var apiKey = string.IsNullOrEmpty(config.EncryptedApiKey)
                ? config.ApiKey
                : _configManager.Decrypt(config.EncryptedApiKey);
            ApiKeyInput.Password = apiKey;

            // DeepL API Key
            var deeplKey = string.IsNullOrEmpty(config.DeepLApiKeyEncrypted)
                ? config.DeepLApiKey
                : _configManager.Decrypt(config.DeepLApiKeyEncrypted);
            DeepLApiKeyInput.Password = deeplKey;

            // Google Translate API Key
            GoogleApiKeyInput.Text = config.GoogleTranslateApiKey;

            // Midjourney 配置
            MidjourneyApiUrlInput.Text = config.MidjourneyApiUrl;
            MidjourneyCallbackUrlInput.Text = config.MidjourneyCallbackUrl;
            MidjourneyEnabledCheckBox.IsChecked = config.MidjourneyEnabled;

            for (int i = 0; i < AIProviderSelector.Items.Count; i++)
            {
                if (AIProviderSelector.Items[i] is System.Windows.Controls.ComboBoxItem item &&
                    item.Content.ToString() == config.SelectedAIProvider)
                {
                    AIProviderSelector.SelectedIndex = i;
                    break;
                }
            }

            // 翻译服务选择
            for (int i = 0; i < TranslationServiceSelector.Items.Count; i++)
            {
                if (TranslationServiceSelector.Items[i] is System.Windows.Controls.ComboBoxItem item &&
                    item.Content.ToString() == config.SelectedTranslationProvider)
                {
                    TranslationServiceSelector.SelectedIndex = i;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _configManager.LogError($"加载配置失败: {ex.Message}");
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // ==================== 工具箱 Tab ====================

    private void ConvertToCurves_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _toolboxService.ConvertToCurves();
        }
        catch (Exception ex)
        {
            _cdrHelper.ShowError($"转曲失败: {ex.Message}");
        }
    }

    private async void ExportPNG_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StatusText.Text = "正在导出...";
                await _toolboxService.ExportSelectedAsPNGAsync(dialog.SelectedPath, (current, total) =>
                {
                    Dispatcher.Invoke(() => StatusText.Text = $"已导出 {current}/{total}...");
                });
            }
        }
        catch (Exception ex)
        {
            _cdrHelper.ShowError($"导出失败: {ex.Message}");
        }
    }

    private async void ExportJPG_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StatusText.Text = "正在导出...";
                await _toolboxService.ExportSelectedAsJPGAsync(dialog.SelectedPath, 90, (current, total) =>
                {
                    Dispatcher.Invoke(() => StatusText.Text = $"已导出 {current}/{total}...");
                });
            }
        }
        catch (Exception ex)
        {
            _cdrHelper.ShowError($"导出失败: {ex.Message}");
        }
    }

    private void SmartAlign_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _toolboxService.SmartAlign("center");
        }
        catch (Exception ex)
        {
            _cdrHelper.ShowError($"对齐失败: {ex.Message}");
        }
    }

    private void AlignCenter_Click(object sender, RoutedEventArgs e) => _toolboxService.SmartAlign("center");
    private void AlignLeft_Click(object sender, RoutedEventArgs e) => _toolboxService.SmartAlign("left");
    private void AlignRight_Click(object sender, RoutedEventArgs e) => _toolboxService.SmartAlign("right");
    private void AlignTop_Click(object sender, RoutedEventArgs e) => _toolboxService.SmartAlign("top");
    private void AlignBottom_Click(object sender, RoutedEventArgs e) => _toolboxService.SmartAlign("bottom");
    private void AlignHCenter_Click(object sender, RoutedEventArgs e) => _toolboxService.SmartAlign("hcenter");
    private void AlignVCenter_Click(object sender, RoutedEventArgs e) => _toolboxService.SmartAlign("vcenter");
    private void AlignToPageCenter_Click(object sender, RoutedEventArgs e) => _toolboxService.AlignToPageCenter();

    private void CreateTestRectangle_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var shape = _cdrHelper.CreateRectangle();
            if (shape != null)
                _cdrHelper.ShowMessage("测试矩形已创建在页面中心");
        }
        catch (Exception ex)
        {
            _cdrHelper.ShowError($"创建矩形失败: {ex.Message}");
        }
    }

    private void GetSelectedInfo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var shapes = _cdrHelper.GetSelectedShapes().ToList();
            if (shapes.Count == 0)
            {
                _cdrHelper.ShowMessage("请先选中对象");
                return;
            }

            var info = $"选中了 {shapes.Count} 个对象:\n";
            foreach (var shape in shapes.Take(5))
            {
                info += $"- 类型: {shape.Type}, 位置: ({shape.CenterX:F0}, {shape.CenterY:F0})\n";
            }
            if (shapes.Count > 5) info += $"... 还有 {shapes.Count - 5} 个";

            _cdrHelper.ShowMessage(info);
        }
        catch (Exception ex)
        {
            _cdrHelper.ShowError($"获取信息失败: {ex.Message}");
        }
    }

    // ==================== AI 文案 Tab ====================

    private async void GenerateText_Click(object sender, RoutedEventArgs e)
    {
        var prompt = PromptInput.Text;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            _cdrHelper.ShowMessage("请输入提示词");
            return;
        }

        try
        {
            GenerateTextBtn.IsEnabled = false;
            GenerateTextBtn.Content = "生成中...";
            StatusText.Text = "正在生成文案，请稍候...";

            var config = _configManager.Load();
            var service = _aiServiceFactory.GetService(config.SelectedAIProvider);

            string result;
            if (UseContextCheckBox.IsChecked == true)
            {
                var context = _textSelectionService.GetSelectedTextContent();
                ContextText.Text = context;
                result = await service.GenerateTextWithContextAsync(prompt, context, config.ModelName);
            }
            else
            {
                result = await service.GenerateTextAsync(prompt, config.ModelName);
            }

            // 将结果插入到 CDR 文档
            var page = _cdrHelper.GetActivePage();
            if (page != null)
            {
                var textShape = page.CreateArtisticText(
                    page.CenterX - 2000,
                    page.CenterY,
                    result,
                    cdrTextAlignment.cdrCenterAlignment);
            }

            StatusText.Text = "生成成功！";
            _configManager.Log($"文案生成成功: {prompt.Substring(0, Math.Min(20, prompt.Length))}...");
        }
        catch (Exception ex)
        {
            StatusText.Text = $"生成失败: {ex.Message}";
            _configManager.LogError($"文案生成失败: {ex.Message}");
        }
        finally
        {
            GenerateTextBtn.IsEnabled = true;
            GenerateTextBtn.Content = "✨ 生成文案";
        }
    }

    private void LoadContext_Click(object sender, RoutedEventArgs e)
    {
        var context = _textSelectionService.GetSelectedTextContent();
        ContextText.Text = context;
    }

    private void ModelSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // 可根据选择的模型更新提示
    }

    // ==================== AI 绘图 Tab ====================

    private async void GenerateImage_Click(object sender, RoutedEventArgs e)
    {
        var prompt = ImagePrompt.Text;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            _cdrHelper.ShowMessage("请输入提示词");
            return;
        }

        var sizeItem = SizeSelector.SelectedItem as System.Windows.Controls.ComboBoxItem;
        var size = sizeItem?.Content?.ToString() ?? "512x512";

        try
        {
            GenerateImageBtn.IsEnabled = false;
            GenerateImageBtn.Content = "生成中...";
            ImageStatusText.Text = "正在生成图片，请稍候...";

            var config = _configManager.Load();
            var service = _aiServiceFactory.GetImageService();

            if (service == null)
            {
                ImageStatusText.Text = "当前 AI 提供商不支持图片生成";
                return;
            }

            var base64Image = await service.GenerateImageAsync(prompt, NegativePrompt.Text, size);

            // 导入到 CDR
            await ImportBase64ImageToCDR(base64Image);

            ImageStatusText.Text = "图片生成成功！";
            _configManager.Log($"图片生成成功: {prompt.Substring(0, Math.Min(20, prompt.Length))}...");
        }
        catch (Exception ex)
        {
            ImageStatusText.Text = $"生成失败: {ex.Message}";
            _configManager.LogError($"图片生成失败: {ex.Message}");
        }
        finally
        {
            GenerateImageBtn.IsEnabled = true;
            GenerateImageBtn.Content = "🎨 生成图片";
        }
    }

    private async void ImageToImage_Click(object sender, RoutedEventArgs e)
    {
        var shapes = _cdrHelper.GetSelectedShapes().ToList();
        var bitmapShape = shapes.FirstOrDefault(s => s.Type == Corel.Interop.VGCore.cdrShapeType.cdrBitmapShape);

        if (bitmapShape == null)
        {
            _cdrHelper.ShowMessage("请先选中一张位图");
            return;
        }

        var prompt = ImagePrompt.Text;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            _cdrHelper.ShowMessage("请输入提示词");
            return;
        }

        var sizeItem = SizeSelector.SelectedItem as System.Windows.Controls.ComboBoxItem;
        var size = sizeItem?.Content?.ToString() ?? "512x512";

        try
        {
            GenerateImageBtn.IsEnabled = false;
            GenerateImageBtn.Content = "处理中...";
            ImageStatusText.Text = "正在进行图生图处理...";

            // 导出选中图片为临时文件
            var tempInputPath = Path.Combine(Path.GetTempPath(), $"input_{Guid.NewGuid()}.png");
            var tempOutputPath = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid()}.png");

            bitmapShape.Export(tempInputPath, Corel.Interop.VGCore.cdrExportFilterType.cdrPNG);

            var imageBase64 = Convert.ToBase64String(await File.ReadAllBytesAsync(tempInputPath));

            var config = _configManager.Load();
            var service = _aiServiceFactory.GetImageService();

            if (service == null)
            {
                ImageStatusText.Text = "当前 AI 提供商不支持图片生成";
                return;
            }

            var resultBase64 = await service.GenerateImageToImageAsync(imageBase64, prompt, NegativePrompt.Text, size);

            var resultBytes = Convert.FromBase64String(resultBase64);
            await File.WriteAllBytesAsync(tempOutputPath, resultBytes);

            // 导入新图片
            var doc = _cdrHelper.GetActiveDocument();
            if (doc != null)
            {
                var newShape = doc.ActivePage.Import(tempOutputPath);
                newShape.SetPosition(
                    bitmapShape.CenterX - newShape.Size.Width / 2,
                    bitmapShape.CenterY - newShape.Size.Height / 2);
                bitmapShape.Delete();
            }

            // 清理临时文件
            File.Delete(tempInputPath);
            File.Delete(tempOutputPath);

            ImageStatusText.Text = "图生图处理成功！";
        }
        catch (Exception ex)
        {
            ImageStatusText.Text = $"处理失败: {ex.Message}";
            _configManager.LogError($"图生图处理失败: {ex.Message}");
        }
        finally
        {
            GenerateImageBtn.IsEnabled = true;
            GenerateImageBtn.Content = "🎨 生成图片";
        }
    }

    private async Task ImportBase64ImageToCDR(string base64Data)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"sd_output_{Guid.NewGuid()}.png");
        var imageData = Convert.FromBase64String(base64Data);
        await File.WriteAllBytesAsync(tempPath, imageData);

        try
        {
            var shape = _cdrHelper.ImportImage(tempPath);
            if (shape != null)
            {
                var page = _cdrHelper.GetActivePage();
                if (page != null)
                {
                    shape.SetPosition(
                        page.CenterX - shape.Size.Width / 2,
                        page.CenterY - shape.Size.Height / 2);
                }
            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // ==================== 翻译 Tab ====================

    private async void Translate_Click(object sender, RoutedEventArgs e)
    {
        var text = TranslateInput.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            _cdrHelper.ShowMessage("请输入要翻译的文本");
            return;
        }

        var langItem = TargetLangSelector.SelectedItem as System.Windows.Controls.ComboBoxItem;
        var targetLang = langItem?.Tag?.ToString() ?? "ZH";

        var providerItem = TranslationServiceSelector.SelectedItem as System.Windows.Controls.ComboBoxItem;
        var provider = providerItem?.Content?.ToString() ?? "DeepL";

        try
        {
            TranslateBtn.IsEnabled = false;
            TranslateBtn.Content = "翻译中...";

            var service = _translationServiceFactory.GetService(provider);
            var result = await service.TranslateAsync(text, targetLang);

            TranslateOutput.Text = result;
        }
        catch (Exception ex)
        {
            TranslateOutput.Text = $"翻译失败: {ex.Message}";
            _configManager.LogError($"翻译失败: {ex.Message}");
        }
        finally
        {
            TranslateBtn.IsEnabled = true;
            TranslateBtn.Content = "🌐 翻译";
        }
    }

    // ==================== 设置 Tab ====================

    private void SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var config = new AppConfig
            {
                BaseURL = BaseURLInput.Text,
                ApiKey = ApiKeyInput.Password,
                ModelName = ModelNameInput.Text,
                TimeoutSeconds = int.TryParse(TimeoutInput.Text, out var timeout) ? timeout : 120,
                SelectedAIProvider = (AIProviderSelector.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "OpenAI",
                DefaultImageSize = DefaultImageSizeInput.Text,
                DefaultExportFormat = (DefaultExportFormatSelector.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "PNG",
                DeepLApiKey = DeepLApiKeyInput.Password,
                GoogleTranslateApiKey = GoogleApiKeyInput.Text,
                MidjourneyEnabled = MidjourneyEnabledCheckBox.IsChecked ?? false,
                MidjourneyApiUrl = MidjourneyApiUrlInput.Text,
                MidjourneyCallbackUrl = MidjourneyCallbackUrlInput.Text,
                SelectedTranslationProvider = (TranslationServiceSelector.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "DeepL"
            };

            _configManager.Save(config);
            ConfigStatusText.Text = "设置已保存！";
        }
        catch (Exception ex)
        {
            ConfigStatusText.Text = $"保存失败: {ex.Message}";
            _configManager.LogError($"配置保存失败: {ex.Message}");
        }
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var config = new AppConfig
            {
                BaseURL = BaseURLInput.Text,
                ApiKey = ApiKeyInput.Password,
                ModelName = ModelNameInput.Text
            };
            _configManager.Save(config);

            ConfigStatusText.Text = "正在测试连接...";

            var service = _aiServiceFactory.GetService();
            var success = await service.TestConnectionAsync();

            ConfigStatusText.Text = success ? "连接成功！" : "连接失败，请检查 API Key 和 URL";
        }
        catch (Exception ex)
        {
            ConfigStatusText.Text = $"连接失败: {ex.Message}";
        }
    }

    private void AIProviderSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var selected = (AIProviderSelector.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "OpenAI";

        switch (selected)
        {
            case "OpenAI":
                BaseURLInput.Text = "https://api.openai.com/v1";
                ModelNameInput.Text = "gpt-4o";
                break;
            case "Claude":
                BaseURLInput.Text = "https://api.anthropic.com";
                ModelNameInput.Text = "claude-3-sonnet-20240229";
                break;
            case "Stable Diffusion":
                BaseURLInput.Text = "http://localhost:7860";
                ModelNameInput.Text = "stable-diffusion";
                break;
            case "Midjourney":
                BaseURLInput.Text = "https://api.midjourney.com";
                ModelNameInput.Text = "midjourney";
                break;
        }
    }
}