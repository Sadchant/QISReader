using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Collections;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;
using System.Net.Http.Headers;

// alte Experimentier-Klasse
namespace QISReader.Model
{

    class Scraper2
    {
        static string baseurl = "https://qis.hs-rm.de/qisserver/rds?state=";
        static string username = "mabeusch";
        static string password = "**********";

        public async Task<string> ClientScraper()
        {
            string  tempString;

            CookieContainer cookieContainer = new CookieContainer() {  };
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseCookies = false;
            //handler.CookieContainer = cookieContainer;
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(baseurl);
            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2");

            HttpResponseMessage response;
            response = client.GetAsync(baseurl + "user&type=0").Result;

            

            HttpResponseHeaders headers = response.Headers;
            string teil = headers.GetValues("set-cookie").ToArray()[0];

            Match match1 = new Regex(@"JSESSIONID=(.*?);").Match(teil);
            if (!match1.Success)
                Debug.WriteLine(":(");

            String cookieInhalt = match1.Groups[1].Value;
            //handler.CookieContainer.Add(response.RequestMessage.RequestUri, new Cookie("JSESSIONID", cookieInhalt));
            //handler.CookieContainer.SetCookies(new Cookie("set-cookie", cookieInhalt));

            var dong = handler.CookieContainer.GetCookies(new Uri(baseurl));

            string rs = "";
            foreach (var plong in dong)
            {
                rs += plong.ToString();
            }


            //string headerString = "";
            //foreach (var ding in headers)
            //{
            //    headerString += ding.Key + ": ";
            //    foreach (var inhalt in ding.Value)
            //    {
            //        headerString += inhalt + ", ";
            //    }
            //    headerString += ";";
            //}


            //##### Anmelden
            FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
            {
                    //the name of the form values must be the name of <input /> tags of the login form, in this case the tag is <input type="text" name="username">
                new KeyValuePair<string, string>("asdf", username),
                new KeyValuePair<string, string>("fdsa", password),
                new KeyValuePair<string, string>("submit", "Anmelden"),
            });
            response = client.PostAsync(baseurl + "user&type=1&category=auth.login&startpage=portal.vm&breadCrumbSource=portal", postData).Result;

            //Cookie manuell setzen

            

            //---
            tempString = await response.Content.ReadAsStringAsync();
            save("nachAnmeldung.html", tempString);
            //---

            //##### auf Prüfungsverwaltung gehen
            response = client.GetAsync(baseurl + "change&type=1&moduleParameter=studyPOSMenu&next=menu.vm&xml=menu").Result;

            //response = client.GetAsync(baseurl + "change&type=1&moduleParameter=studyPOSMenu&nextdir=change&next=menu.vm&subdir=applications&xml=menu&purge=y&navigationPosition=functions%2CstudyPOSMenu&breadcrumb=studyPOSMenu&topitem=functions&subitem=studyPOSMenu").Result;

            tempString = await response.Content.ReadAsStringAsync();
            //---
            save("Prüfungsverwaltung.html", tempString);
            //---

            //---
            Regex regex = new Regex(@";asi=(.*?)""");
            Match match = regex.Match(tempString);
            string asiContent = "";
            if (match.Success)
            {
                asiContent = match.Groups[1].Value;
            }

            //##### auf Notenspiegel gehen
            response = client.GetAsync(baseurl + "notenspiegelStudent&next=tree.vm&nextdir=qispos/notenspiegel/student&menuid=notenspiegelStudent&breadcrumb=notenspiegel&breadCrumbSource=menu&asi=" + asiContent).Result;
            tempString = await response.Content.ReadAsStringAsync();
            //---
            //save("Notenspiegel.html", tempString);
            //---
            regex = new Regex(@"Aabschl\%3D(\d*)");
            match = regex.Match(tempString);
            string degree = "";
            if (match.Success)
            {
                degree = match.Groups[1].Value;
            }

            //##### auf Info gehen
            response = client.GetAsync(baseurl + "notenspiegelStudent&next=list.vm&nextdir=qispos/notenspiegel/student&createInfos=Y&struct=auswahlBaum&nodeID=auswahlBaum%7Cabschluss%3Aabschl%3D" + degree + "%2Cstgnr%3D1%7Cstudiengang%3Astg%3DIDB&expand=0&asi=" + asiContent).Result;
            //---
            tempString = await response.Content.ReadAsStringAsync();
            //save("NotenQISInfo.html", tempString);
            return tempString;
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

        /*public async void GradesCopy()
        {
            string tempString;
            // ##### auf Webseite gehen und Cookie erhalten            
            CookieContainer cookieContainer = new CookieContainer();
            HttpWebRequest request = WebRequest.Create(baseurl + "user&type=0") as HttpWebRequest;
            request.CookieContainer = cookieContainer;

            // befüllt dabei den Cookie-Container des Requests
            WebResponse response = await request.GetResponseAsync();

            string postData = String.Format("asdf={0}&fdsa={1}&submit=Anmelden", username, password);

            //##### Anmelden
            // den befüllten Cookiecontainer in den zweiten Request einbauen
            HttpWebRequest secondRequest = WebRequest.Create(baseurl + "user&type=1&category=auth.login&startpage=portal.vm&breadCrumbSource=portal") as HttpWebRequest;
            secondRequest.CookieContainer = cookieContainer;
            secondRequest.Method = "POST";
            //secondRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            secondRequest.ContentType = "application/x-www-form-urlencoded";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            secondRequest.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = secondRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            HttpWebResponse secondResponse = (HttpWebResponse)secondRequest.GetResponse();
            using (StreamReader sr = new StreamReader(secondResponse.GetResponseStream()))
            {
                tempString = sr.ReadToEnd();
            }
            WriteFile("nachAnmeldung.html", tempString);

            //##### auf Prüfungsverwaltung gehen
            HttpWebRequest thirdRequest = WebRequest.Create(baseurl + "change&type=1&moduleParameter=studyPOSMenu&next=menu.vm&xml=menu") as HttpWebRequest;
            thirdRequest.CookieContainer = cookieContainer;

            HttpWebResponse thirdResponse = (HttpWebResponse)thirdRequest.GetResponse();
            using (StreamReader sr = new StreamReader(thirdResponse.GetResponseStream()))
            {
                tempString = sr.ReadToEnd();
            }
            WriteFile("Prüfungsverwaltung.html", tempString);

            Regex regex = new Regex(@";asi=(.*?)""");
            Match match = regex.Match(tempString);
            string asiContent = "";
            if (match.Success)
            {
                asiContent = match.Groups[1].Value;
            }

            HttpWebRequest fourthRequest = WebRequest.Create(baseurl + "notenspiegelStudent&next=tree.vm&nextdir=qispos/notenspiegel/student&menuid=notenspiegelStudent&breadcrumb=notenspiegel&breadCrumbSource=menu&asi=" + asiContent) as HttpWebRequest;
            fourthRequest.CookieContainer = cookieContainer;
            HttpWebResponse fourthResponse = (HttpWebResponse)fourthRequest.GetResponse();

            using (StreamReader sr = new StreamReader(fourthResponse.GetResponseStream()))
            {
                tempString = sr.ReadToEnd();
            }
            WriteFile("Notenspiegel.html", tempString);

            regex = new Regex(@"Aabschl\%3D(\d*)");
            match = regex.Match(tempString);
            string degree = "";
            if (match.Success)
            {
                degree = match.Groups[1].Value;
            }

            HttpWebRequest fifthRequest = WebRequest.Create(baseurl + "notenspiegelStudent&next=list.vm&nextdir=qispos/notenspiegel/student&createInfos=Y&struct=auswahlBaum&nodeID=auswahlBaum%7Cabschluss%3Aabschl%3D" + degree + "%2Cstgnr%3D1%7Cstudiengang%3Astg%3DIDB&expand=0&asi=" + asiContent) as HttpWebRequest;
            fifthRequest.CookieContainer = cookieContainer;
            HttpWebResponse fifthResponse = (HttpWebResponse)fifthRequest.GetResponse();

            using (StreamReader sr = new StreamReader(fifthResponse.GetResponseStream()))
            {
                tempString = sr.ReadToEnd();
            }
            WriteFile("AbschlussBachelor.html", tempString);

        }*/

    }
}

/*          
            HttpResponseHeaders headers = response.Headers;
            string headerString = "";
            foreach (var ding in headers)
            {
                headerString += ding.Key + ": ";
                foreach (var inhalt in ding.Value)
                {
                    headerString += inhalt + ", ";
                }
                headerString += ";";
            }
*/