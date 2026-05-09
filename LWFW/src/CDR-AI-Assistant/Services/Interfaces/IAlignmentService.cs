using Corel.Interop.VGCore;

namespace CDR_AI_Assistant.Services.Interfaces;

public interface IAlignmentService
{
    void AlignCenter(IEnumerable<Shape> shapes, bool horizontal);
    void AlignLeft(IEnumerable<Shape> shapes);
    void AlignRight(IEnumerable<Shape> shapes);
    void AlignTop(IEnumerable<Shape> shapes);
    void AlignBottom(IEnumerable<Shape> shapes);
    void AlignToPageCenter(IEnumerable<Shape> shapes);
}