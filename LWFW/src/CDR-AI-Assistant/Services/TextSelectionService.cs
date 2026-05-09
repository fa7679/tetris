namespace CDR_AI_Assistant.Services;

public class TextSelectionService
{
    private readonly CDRHelper _cdrHelper;

    public TextSelectionService(CDRHelper cdrHelper)
    {
        _cdrHelper = cdrHelper;
    }

    public string GetSelectedTextContent()
    {
        return _cdrHelper.GetSelectedTextContent();
    }
}