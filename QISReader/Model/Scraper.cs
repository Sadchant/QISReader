using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace QISReader.Model
{

    public class WrongLoginException : System.Exception { }
    public class ScrapQISException : System.Exception { }

    public class Scraper
    {
        public string Baseurl { get; set; }
        private string hsrmbaseurl = "https://qis.hs-rm.de/qisserver/rds?state=";
        public string Username { get; set; }
        public string Password { get; set; }

        private HttpClient client;
        HttpResponseMessage response;
        private string resultPage;

        private string aktAsi;
        private string aktDegree;

        // gibt den mit Cookie initialisierten
        public async Task Login()
        {
            HttpClientHandler handler = new HttpClientHandler();
            client = new HttpClient(handler);
            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2");

           // Anmeldeform auf QIS-Seite ausfüllen
           FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
           {
               new KeyValuePair<string, string>("asdf", Username),
               new KeyValuePair<string, string>("fdsa", Password),
               new KeyValuePair<string, string>("submit", "Anmelden"),
           });
            response = client.PostAsync(Baseurl + "user&type=1&category=auth.login&startpage=portal.vm&breadCrumbSource=portal", postData).Result;
            // die zurückgegebene HTML-Seite auslesen     
            resultPage = await response.Content.ReadAsStringAsync();
            // überprüfen, ob die Anmeldung fehlgeschlagen ist (liegt höchstwahrscheinlich an falschem Login)
            if (resultPage.IndexOf("Anmeldung fehlgeschlagen") != -1)
            {
                throw new WrongLoginException();
            }
        }

        // navigiert bis zur Notenseite und gibt diese als String zurück
        public async Task<string> NavigateQis()
        {
            //##### auf Prüfungsverwaltung gehen
            response = await client.GetAsync(Baseurl + "change&type=1&moduleParameter=studyPOSMenu&next=menu.vm&xml=menu");

            resultPage = await response.Content.ReadAsStringAsync();

            Regex regex = new Regex(@";asi=(.*?)""");
            Match match = regex.Match(resultPage);
            string asiContent = "";
            if (!match.Success)
                throw new ScrapQISException();

            asiContent = match.Groups[1].Value;
            aktAsi = asiContent;
            //##### auf Notenspiegel gehen
            response = client.GetAsync(Baseurl + "notenspiegelStudent&next=tree.vm&nextdir=qispos/notenspiegel/student&menuid=notenspiegelStudent&breadcrumb=notenspiegel&breadCrumbSource=menu&asi=" + asiContent).Result;
            resultPage = await response.Content.ReadAsStringAsync();

            regex = new Regex(@"Aabschl\%3D(\d*)");
            match = regex.Match(resultPage);
            string degree = "";
            if (!match.Success)
                throw new ScrapQISException();

            degree = match.Groups[1].Value;
            aktDegree = degree;
            //##### auf Info gehen
            response = client.GetAsync(Baseurl + "notenspiegelStudent&next=list.vm&nextdir=qispos/notenspiegel/student&createInfos=Y&struct=auswahlBaum&nodeID=auswahlBaum%7Cabschluss%3Aabschl%3D" + degree + "%2Cstgnr%3D1%7Cstudiengang%3Astg%3DIDB&expand=0&asi=" + asiContent).Result;

            resultPage = await response.Content.ReadAsStringAsync();
            //save("Info.html", resultPage);

            return resultPage;
        }

        private async void save(string name, string input)
        {
            try
            {
                //create file in public folder
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await storageFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

                //write sring to created file
                await FileIO.WriteTextAsync(sampleFile, input);

                //get asets folder
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");

                //move file from public folder to assets
                await sampleFile.MoveAsync(assetsFolder, "new_file_name.txt", NameCollisionOption.ReplaceExisting);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error: " + ex);
            }
        }

        public async Task<string> navigateToNotenSpiegel(string link)
        {
            /*int asiIndex = link.IndexOf(";asi=");
            //Debug.WriteLine(link.Substring(0, asiIndex));

            int degreeAfterIndex = link.IndexOf("%2Cstgnr");
            int Aabluschl3DIndex = link.IndexOf("Aabschl%3D");
            Debug.WriteLine(link.Length);
            string ende = link.Substring(degreeAfterIndex, link.Length - asiIndex);
            string newLink = link.Substring(0, Aabluschl3DIndex + 10) + aktDegree + link.Substring(degreeAfterIndex, link.Length - degreeAfterIndex);
            newLink = newLink.Substring(0, asiIndex + 5) + aktAsi;
            Debug.WriteLine(newLink);*/
            string newlink = link.Replace("amp;", ""); // sauberen string ohne html-Sonderzeichen-Darstellung verschicken!
            try
            {
                response = await client.GetAsync(newlink);
                resultPage = await response.Content.ReadAsStringAsync();
                return resultPage;
            }
            catch
            {
                throw new ScrapQISException();
            }
        }
    }
}