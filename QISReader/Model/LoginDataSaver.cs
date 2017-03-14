using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace QISReader.Model
{
    public class LoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class LoginDataSaver
    {
        private string resourceName = "QisReader";
        PasswordVault vault = new PasswordVault();

        public LoginData GetLoginData()
        {
            PasswordCredential loginCredential = GetCredentialFromLocker();
            if (loginCredential != null)
                return new LoginData { Username = loginCredential.UserName, Password = loginCredential.Password };
            else
                return null;
        }

        public void SetLoginData(string username, string password)
        {
            vault.Add(new PasswordCredential(resourceName, username, password));
        }

        private PasswordCredential GetCredentialFromLocker()
        {
            PasswordCredential credential = null;
            IReadOnlyList<PasswordCredential> credentialList = null;
            try
            {
                credentialList = vault.FindAllByResource(resourceName);
            }
            catch // wenn noch keine Daten hinterlegt sind wird eine Exception geworfen
            {
                return null;
            }
            
            // es sollte immer nur einen Eintrag geben, da entweder einmal eingeloggt oder einmal ausgeloggt wird, wenn ausgeloggt wird das null-credential returned
            if (credentialList.Count == 1)
            {
                credential = credentialList[0];
            }
            return credential;
        }

        public void Logout()
        {
            PasswordCredential loginCredential = GetCredentialFromLocker();
            if (loginCredential != null)
            {
                vault.Remove(loginCredential);
            }
            
        }
    }
}
