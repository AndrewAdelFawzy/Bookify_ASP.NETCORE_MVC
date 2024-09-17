using Boookify.Web.Core.Models;

namespace Boookify.Web.Services
{
	public class ImageService : IImageService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		private List<string> _allowedExtentions = new() { ".jpg", ".png", ".jpeg" };
		private int _maxAllowedSize = 2097152;// 2MB(inBytes) 2 * 1024 * 1024

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }


		public async Task<(bool isUploaded, string? errorMessage)> UploadASync(IFormFile image, string imageName, string folderPath)
		{
			
				var extention = Path.GetExtension(image.FileName);

				if (image.Length > _maxAllowedSize)
					return (isUploaded: false, errorMessage: Errors.Filesize);

				if (!_allowedExtentions.Contains(extention))		
					return (isUploaded: false, errorMessage: Errors.ExtntionNotAllowed);

				var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);

				using var stream = File.Create(path);
				await image.CopyToAsync(stream);
				stream.Dispose();

			return (isUploaded: true, errorMessage: null);
		
		}

		public void Delete(string imagePath)
		{
			var oldImagePath = Path.Combine($"{_webHostEnvironment.WebRootPath}{imagePath}");
			if (File.Exists(oldImagePath))
				File.Delete(oldImagePath);
		}
	}
}
