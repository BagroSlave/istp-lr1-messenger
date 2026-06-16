using Microsoft.AspNetCore.Http;

namespace MessengerApp.Data;

// Допоміжний клас: перетворює завантажене зображення на data-URI (base64) із обмеженням розміру.
public static class ImageHelper
{
    // Максимальний розмір зображення ~3 МБ.
    public const long MaxBytes = 3 * 1024 * 1024;

    // Повертає рядок data:image/...;base64,... або null, якщо файл відсутній/завеликий/не зображення.
    public static async Task<string?> ToDataUriAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > MaxBytes) return null;

        var contentType = file.ContentType;
        if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("image/"))
            return null;

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        return $"data:{contentType};base64,{base64}";
    }
}
