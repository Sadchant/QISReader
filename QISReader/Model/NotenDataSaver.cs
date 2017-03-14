using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using Windows.Storage;
using System.IO;

using System.Diagnostics;
using Windows.Storage.Streams;

namespace QISReader.Model
{
    // speichert und läd Notenliste mitHilfe von Json-Files
    class NotenDataSaver
    {
        private string notenFilename = "noten.json";

        // speichert die angezeigte Notenliste, die aus einer Fach-Liste besteht in ein json-File ab
        public async void SaveNoten(List<Fach> fachList)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile;

            // wenn es den File noch nicht gibt, erzeuge ihn
            if (await storageFolder.TryGetItemAsync(notenFilename) == null)
                storageFile = await storageFolder.CreateFileAsync(notenFilename);
            else // ansonsten hole den File
                storageFile = await storageFolder.GetFileAsync(notenFilename);
            
            // der Stream ist dazu da, um ihn im JsonSerializer zum Schreiben zu benutzen
            using (Stream stream = await storageFile.OpenStreamForWriteAsync())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Fach>));
                serializer.WriteObject(stream, fachList);
            }                
        }

        // läd die Fach-Liste aus dem json-File
        public async Task<List<Fach>> LoadNoten()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile;
            if (await storageFolder.TryGetItemAsync(notenFilename) == null)
                return null;

            storageFile = await storageFolder.GetFileAsync(notenFilename);
            using (Stream stream = await storageFile.OpenStreamForReadAsync())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Fach>));
                return (List<Fach>)serializer.ReadObject(stream);
            }
        }        
    }
}
