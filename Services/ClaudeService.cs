using Anthropic;
using Anthropic.Models.Messages;
using WhatsAppErrorAssistant.Models;

namespace WhatsAppErrorAssistant.Services;

public class ClaudeService
{
    private readonly AnthropicClient _client;
    private const string ModelId = "claude-haiku-4-5";

    private const string SystemPrompt = """
        Sen uzman bir WhatsApp teknik destek asistanısın.
        Kullanıcıların paylaştığı WhatsApp hata mesajlarını veya log çıktılarını analiz ederek:

        1. **Hata Açıklaması**: Hatanın ne anlama geldiğini sade bir dille açıkla.
        2. **Olası Nedenler**: Hatanın en yaygın 2-4 nedenini maddeler hâlinde listele.
        3. **Çözüm Önerileri**: Kullanıcının uygulayabileceği adım adım çözümleri sun.
        4. **Önem Derecesi**: Hatanın ciddiyetini belirt (Düşük / Orta / Yüksek).

        Yanıtlarını Türkçe ver. Teknik jargonu minimumda tut; anlaşılır ve öz ol.
        Eğer gönderilen metin bir WhatsApp hatasıyla ilgili değilse, bunu nazikçe belirt.
        """;

    public ClaudeService(IConfiguration configuration)
    {
        var apiKey = configuration["CLAUDE_API_KEY"]
            ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
            ?? throw new InvalidOperationException(
                "CLAUDE_API_KEY bulunamadı. Lütfen ortam değişkenini veya appsettings.json dosyasını kontrol edin.");

        _client = new AnthropicClient { ApiKey = apiKey };
    }

    public async Task<AnalyzeResponse> AnalyzeAsync(AnalyzeRequest request)
    {
        var userContent = string.IsNullOrWhiteSpace(request.Context)
            ? request.Message
            : $"Hata Mesajı:\n{request.Message}\n\nEk Bağlam:\n{request.Context}";

        var parameters = new MessageCreateParams
        {
            Model = ModelId,
            MaxTokens = 1024,
            System = SystemPrompt,
            Messages =
            [
                new() { Role = Role.User, Content = userContent }
            ]
        };

        var response = await _client.Messages.Create(parameters);

        var analysisText = response.Content
            .Select(b => b.Value)
            .OfType<TextBlock>()
            .FirstOrDefault()?.Text
            ?? "Analiz tamamlanamadı. Lütfen tekrar deneyin.";

        return new AnalyzeResponse
        {
            Analysis = analysisText,
            Model = ModelId,
            InputMessage = request.Message,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
