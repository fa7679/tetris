namespace CDR_AI_Assistant.Models;

public class ShapeInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Width { get; set; }
    public double Height { get; set; }
    public double CenterX { get; set; }
    public double CenterY { get; set; }
}