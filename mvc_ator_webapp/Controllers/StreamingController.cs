using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using mvc_ator_webapp.Filters;
using mvc_ator_webapp.Models;
using mvc_ator_webapp.Utilities;

namespace mvc_ator_webapp.Controllers
{
    public class StreamingController : Controller
    {

        private readonly long _fileSizeLimit;
        private readonly ILogger<StreamingController> _logger;
        private readonly string[] _permittedExtensions = { ".txt", ".in" };
        private readonly string _targetFilePath;
        public StreamingController(ILogger<StreamingController> logger, IConfiguration config)
        {
            _logger = logger;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            // To save physical files to a path provided by configuration:
            _targetFilePath = config.GetValue<string>("StoredFilesPath");

            // To save physical files to the temporary files folder, use:
            //_targetFilePath = Path.GetTempPath();
        }
        // Get the default form options so that we can use them to set the default 
        // limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();


        // The following upload methods:
        //
        // 1. Disable the form value model binding to take control of handling 
        //    potentially large files.
        //
        // 2. Typically, antiforgery tokens are sent in request body. Since we 
        //    don't want to read the request body early, the tokens are sent via 
        //    headers. The antiforgery token filter first looks for tokens in 
        //    the request header and then falls back to reading the body.

        #region snippet_UploadPhysical
        [HttpPost]
        [DisableFormValueModelBinding]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhysical()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File", 
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = 
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File", 
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();


                        using (var inMemStream = new MemoryStream(await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permittedExtensions, _fileSizeLimit)))
                        {

                            if (!ModelState.IsValid)
                            {
                                return BadRequest(ModelState);
                            }


                            RegisteredS3.Process(inMemStream, trustedFileNameForDisplay);
                            _logger.LogInformation(RegisteredS3.retStr +
                                "\nReturn from uploading file '{TrustedFileNameForDisplay}' sent to " +
                                "the registered AWS S3 bucket",
                                trustedFileNameForDisplay);

                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(StreamingController), null);
        }
        #endregion

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = 
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }

    public class FormData
    {
        public string Note { get; set; }
    }
}
