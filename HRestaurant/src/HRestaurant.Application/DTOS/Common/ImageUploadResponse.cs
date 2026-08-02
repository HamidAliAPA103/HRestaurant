namespace HRestaurant.DTOS.Common;

public sealed record ImageUploadResponse(
    string Url,
    string FileName,
    long Size);
