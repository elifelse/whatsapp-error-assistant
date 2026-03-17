namespace WhatsAppErrorAssistant.Models;

public class AnalyzeResponse
{
    /// <summary>
    /// Claude tarafından üretilen analiz metni.
    /// </summary>
    public string Analysis { get; set; } = string.Empty;

    /// <summary>
    /// Kullanılan Claude model adı.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Analiz edilen orijinal mesaj.
    /// </summary>
    public string InputMessage { get; set; } = string.Empty;

    /// <summary>
    /// İsteğin işlendiği UTC zaman damgası.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
