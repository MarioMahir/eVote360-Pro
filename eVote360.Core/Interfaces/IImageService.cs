using Microsoft.AspNetCore.Http;

namespace eVote360.Core.Interfaces.Services;

public interface IImageService
{
    Task<string> SavePartyLogoAsync(
        IFormFile file,
        string siglas);

    void DeleteImage(string imageUrl);

    bool IsValidImage(IFormFile file);
}