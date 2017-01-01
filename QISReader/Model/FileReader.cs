using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace QISReader.Model
{
    class FileReader
    {
        public async Task<string> readNotenPage()
        {
            var notenFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/noten.html"));
            string result = await Windows.Storage.FileIO.ReadTextAsync(notenFile);
            return result;
        }

        public static async Task<string> readNotenSpiegelPage()
        {
            var notenFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/notenspiegel.html"));
            string result = await Windows.Storage.FileIO.ReadTextAsync(notenFile);
            return result;
        }

    }
}
