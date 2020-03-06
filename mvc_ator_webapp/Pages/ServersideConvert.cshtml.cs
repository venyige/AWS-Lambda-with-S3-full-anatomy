using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using mvc_ator_webapp.Utilities;

namespace mvc_ator_webapp.Pages
{

    public class ServersideConvertModel : PageModel
    {
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".txt", ".in" };
        private readonly string _targetFilePath;

        public ServersideConvertModel(IConfiguration config)
        {
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            // To save physical files to a path provided by configuration:
            _targetFilePath = config.GetValue<string>("StoredFilesPath");

        }

        [BindProperty]
        public ServersideConvert FileUpload { get; set; }

        public string Result { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (!ModelState.IsValid)
            {
                Result = "Please change your selection.";

                return Page();
            }

            foreach (var formFile in FileUpload.FormFiles)
            {


                if (!ModelState.IsValid)
                {
                    Result = "Please change your selection.";

                    return Page();
                }

                // For the file name of the uploaded file stored
                // server-side, use Path.GetRandomFileName to generate a safe
                // random file name.
                var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var filePath = Path.Combine(
                    _targetFilePath, trustedFileNameForFileStorage);
                using (MemoryStream arabNumStream = new MemoryStream(await FileHelpers
                .ProcessFormFile<ServersideConvert>(formFile, ModelState, _permittedExtensions, _fileSizeLimit)))
                using (StreamReader reader = new StreamReader(arabNumStream))
                using (StreamWriter fileStream = new StreamWriter(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            if (line.Length == 0)
                                continue;
                            int numToAdd = Int32.Parse(line);
                            if (numToAdd > 0 && numToAdd < 4000)
                            {
                                fileStream.WriteLine(ArabToRoman(numToAdd));
                            }
                            else
                            {

                                Result = "The input file contains number(s) out of range 0 - 4000 n:" + numToAdd.ToString();

                                return Page();
                            }

                        }
                        catch (FormatException e)
                        {
                            Result = "The input file contains non integer-convertible line(s) {0}" + e.Message;
                            return Page();
                        }
                    }

                }
            }
            return RedirectToPage("./Index");
        }
        string ArabToRoman(int arabNum)
        {
            List<RomanDigit> romanDigits = new List<RomanDigit>() {
             new RomanDigit{pT=true,  n=1000, d="M" },
             new RomanDigit{pT=false, n=500,  d="D" },
             new RomanDigit{pT=true,  n=100,  d="C" },
             new RomanDigit{pT=false, n=50,   d="L" },
             new RomanDigit{pT=true,  n=10,   d="X" },
             new RomanDigit{pT=false, n=5,    d="V" },
             new RomanDigit{pT=true,  n=1,    d="I" } };
            var retStr = string.Empty;
            if (arabNum > 0 && arabNum <= 4000)
            {
                while (arabNum > 0)
                {
                    int iiPri = romanDigits.FindIndex(x => x.n <= arabNum); /// Primary index
                    if (iiPri > 0)
                    {
                        int iiNeg = iiPri + (romanDigits[iiPri].pT ? 0 : 1); /// Index of the first element of the reduced number
                        int neg = romanDigits[iiNeg].n;/// The first element of the reduced number
                        int iiRed = romanDigits.FindIndex(x => x.n - neg <= arabNum);/// Index of the largest reduced number 
                        if (iiRed < iiPri)/// If the reduced number is more significant
                        {
                            iiPri = iiRed;
                            arabNum -= (romanDigits[iiPri].n - neg);
                            retStr += (romanDigits[iiNeg].d + romanDigits[iiPri].d);
                        }
                        else
                        {
                            arabNum -= romanDigits[iiPri].n;
                            retStr += romanDigits[iiPri].d;
                        }

                    }
                    else /// If 1000
                    {
                        arabNum -= romanDigits[iiPri].n;
                        retStr += romanDigits[iiPri].d;
                    }

                }
            }
            else
            {
                /// With the actual Main() function, it is never processed
                retStr = "ERROR: The number must be between 0 and 4000";

            }
            return retStr;
        }

    }

    public class ServersideConvert
    {
        [Required]
        [Display(Name="File")]
        public List<IFormFile> FormFiles { get; set; }

        [Display(Name="Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }
    struct RomanDigit
    {
        public bool pT; //isPrimaryType;//is it ten exponential that is in the set of {I, X, C, M}
        public string d; //digit;
        public int n;
    }

}
