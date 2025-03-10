﻿using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using System;

namespace Boookify.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetEmailBody(string imageUrl, string header, string body, string url, string linkTitle)
        {
            var filePath = $"{_webHostEnvironment.WebRootPath}/templates/email.html";
            StreamReader str = new(filePath);

            var template = str.ReadToEnd();
            str.Close();

            return template
                .Replace("[imageUrl]", imageUrl)
                .Replace("[header]", header)
                .Replace("[body]", body)
                .Replace("[url]", url)
                .Replace("[linkTitle]",linkTitle);

            
        }
    }
}
