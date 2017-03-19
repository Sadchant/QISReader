using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace QisReaderClassLibrary
{
    public class JsonManager
    {
        // speichert die angezeigte Notenliste, die aus einer Fach-Liste besteht in ein json-File ab
        public async void Save<T>(T saveThis, string filename)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile;

            // wenn es den File noch nicht gibt, erzeuge ihn
            if (await storageFolder.TryGetItemAsync(filename) == null)
                storageFile = await storageFolder.CreateFileAsync(filename);
            else // ansonsten hole den File
                storageFile = await storageFolder.GetFileAsync(filename);

            // der Stream ist dazu da, um ihn im JsonSerializer zum Schreiben zu benutzen
            using (Stream stream = await storageFile.OpenStreamForWriteAsync())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, saveThis);
            }
        }

        // läd die Fach-Liste aus dem json-File
        public async Task<T> Load<T>(string filename)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile;
            if (await storageFolder.TryGetItemAsync(filename) == null)
                return default(T);

            storageFile = await storageFolder.GetFileAsync(filename);
            using (Stream stream = await storageFile.OpenStreamForReadAsync())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Fach>));
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
