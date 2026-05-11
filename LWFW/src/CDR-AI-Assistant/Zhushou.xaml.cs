using System.Windows;
using System.Windows.Controls;
using CDR_AI_Assistant.Services;

namespace CDR_AI_Assistant;

public partial class Zhushou : UserControl
{
    public Zhushou()
    {
        InitializeComponent();
    }

    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var helper = CDRHelper.GetApplication();
            MessageBox.Show($"CDR Version: {helper.App.Version}", "Success");
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Failed");
        }
    }

    private void ThemeCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (ThemeCheck.IsChecked == true)
        {
            var grid = Content as Grid;
            if (grid != null) grid.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2D, 0x2D, 0x30));
        }
        else
        {
            var grid = Content as Grid;
            if (grid != null) grid.Background = System.Windows.Media.Brushes.White;
        }
    }
}