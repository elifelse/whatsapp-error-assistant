using System.ComponentModel.DataAnnotations;

namespace WhatsAppErrorAssistant.Models;

public class AnalyzeRequest
{
    /// <summary>
    /// Analiz edilecek WhatsApp hata mesajı veya log içeriği.
    /// </summary>
    [Required(ErrorMessage = "Message alanı zorunludur.")]
    [MinLength(3, ErrorMessage = "Message en az 3 karakter olmalıdır.")]
    [MaxLength(4000, ErrorMessage = "Message en fazla 4000 karakter olabilir.")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// İsteğe bağlı: Ek bağlam (örn. cihaz modeli, WhatsApp versiyonu).
    /// </summary>
    [MaxLength(500)]
    public string? Context { get; set; }
}
