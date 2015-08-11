using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Zinal.Screeps.ScriptAPI
{
    public class API
    {
        /// <summary>
        /// If either ImportScripts or ExportScripts return a FALSE then this will be set to the last error that occurred.
        /// </summary>
        public Exception LastError { get; private set; }

        public String Username { get; private set; }
        public String Password { get; private set; }
        public String Branch { get; private set; }
        public String ImportExportFolder { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImportExportFolder">The default folder to import/export scripts to/from</param>
        /// <param name="Branch">The default branch name</param>
        /// <param name="Username">The default username</param>
        /// <param name="Password">The default password</param>
        public API(String ImportExportFolder = null, String Branch = "default", String Username = null, String Password = null)
        {
            this.ImportExportFolder = ImportExportFolder;
            this.Branch = Branch;
            this.Username = Username;
            this.Password = Password;
        }

        public void SetAuthorization(String Username, String Password)
        {
            this.Username = Username;
            this.Password = Password;
        }

        /// <summary>
        /// Import (get) scripts into a folder you specify
        /// </summary>
        /// <param name="ImportFolder">The folder to import the scripts into</param>
        /// <param name="Branch">The branch to import</param>
        /// <param name="Username">The username of your account</param>
        /// <param name="Password">The password of your account</param>
        /// <returns></returns>
        public Boolean ImportScripts(String ImportFolder = null, String Branch = null, String Username = null, String Password = null)
        {
            if (ImportFolder == null)
                ImportFolder = this.ImportExportFolder;

            if (Branch == null)
                Branch = this.Branch;

            if (Username == null)
                Username = this.Username;

            if (Password == null)
                Password = this.Password;

            if (!Directory.Exists(ImportExportFolder))
            {
                this.LastError = new DirectoryNotFoundException("Import folder does not exist");
                return false;
            }

            if (Branch == null || Branch == "")
            {
                this.LastError = new InvalidDataException("Invalid branch specified");
                return false;
            }

            if(Username == null || Password == null)
            {
                this.LastError = new InvalidDataException("You have to specify a username and password");
                return false;
            }

            HttpWebRequest Req;
            HttpWebResponse Res;
            try
            {
                Req = (HttpWebRequest)HttpWebRequest.Create("https://screeps.com/api/user/code?branch=" + Branch);
                Req.Method = "GET";
                Req.ContentType = "application/json; charset=utf-8";
                Req.Headers.Set(HttpRequestHeader.Authorization, GetAuthString(Username, Password));


                Res = (HttpWebResponse)Req.GetResponse();
                StreamReader sr = new StreamReader(Res.GetResponseStream());
                String response = sr.ReadToEnd();
                sr.Close();

                JObject ret = JObject.Parse(response);

                JToken ok;

                if (ret.TryGetValue("ok", out ok))
                {
                    if (ok.Value<int>() == 1)
                    {
                        JObject modules = ret["modules"].Value<JObject>();
                        PutFolderData(ImportFolder, modules);
                        this.LastError = null;
                        return true;
                    }
                }

                JToken error;
                if (ret.TryGetValue("error", out error))
                {
                    this.LastError = new InvalidDataException(error.Value<String>());
                    return false;
                }

                this.LastError = new InvalidDataException("Server sent invalid response: " + response);
                return false;

            }
            catch (Exception ex)
            {
                this.LastError = ex;

                return false;
            }

        }

        /// <summary>
        /// Export (send) scripts from a local folder to Screeps server
        /// </summary>
        /// <param name="ExportFolder">The folder to export the scripts from</param>
        /// <param name="Branch">The branch to import</param>
        /// <param name="Username">The username of your account</param>
        /// <param name="Password">The password of your account</param>
        /// <returns></returns>
        public Boolean ExportScipts(String ExportFolder = null, String Branch = null, String Username = null, String Password = null)
        {
            if (ExportFolder == null)
                ExportFolder = this.ImportExportFolder;

            if (Branch == null)
                Branch = this.Branch;

            if (Username == null)
                Username = this.Username;

            if (Password == null)
                Password = this.Password;

            if (!Directory.Exists(ExportFolder))
            {
                this.LastError = new DirectoryNotFoundException("Import folder does not exist");
                return false;
            }

            if (Branch == null || Branch == "")
            {
                this.LastError = new InvalidDataException("Invalid branch specified");
                return false;
            }

            if (Username == null || Password == null)
            {
                this.LastError = new InvalidDataException("You have to specify a username and password");
                return false;
            }

            HttpWebRequest Req;
            HttpWebResponse Res;
            try
            {
                Req = (HttpWebRequest)HttpWebRequest.Create("https://screeps.com/api/user/code");
                Req.SendChunked = true;
                Req.Method = "POST";
                Req.ContentType = "application/json; charset=utf-8";
                Req.Headers.Set(HttpRequestHeader.Authorization, GetAuthString(Username, Password));
                Req.KeepAlive = false;

                String content = GetFolderData(ExportFolder, Branch);
                byte[] byteContent = Encoding.UTF8.GetBytes(content);
                Req.GetRequestStream().Write(byteContent, 0, byteContent.Length);

                Res = (HttpWebResponse)Req.GetResponse();
                StreamReader sr = new StreamReader(Res.GetResponseStream());
                String response = sr.ReadToEnd();
                sr.Close();

                try
                {
                    JObject ret = JObject.Parse(response);

                    JToken ok;

                    if(ret.TryGetValue("ok", out ok))
                    {
                        if(ok.Value<int>() == 1)
                        {
                            this.LastError = null;
                            return true;
                        }
                    }

                    JToken error;
                    if (ret.TryGetValue("error", out error))
                    {
                        this.LastError = new InvalidDataException(error.Value<String>());
                        return false;
                    }

                    this.LastError = new InvalidDataException("Server sent invalid response: " + response);
                    return false;
                }
                catch
                {
                    this.LastError = new InvalidDataException("Server sent invalid response: " + response);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.LastError = ex;
                return false;
            }
        }

        private String GetFolderData(String folder, String branch)
        {

            JObject data = new JObject();
            data.Add("branch", branch);

            JObject modules = new JObject();

            foreach (String filename in Directory.GetFiles(folder))
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.Extension != ".js")
                    continue;

                StreamReader sr = new StreamReader(filename);
                String content = sr.ReadToEnd();
                sr.Close();

                modules.Add(fi.Name.Replace(fi.Extension, ""), content);

                //modules.Add(content);
            }

            data.Add("modules", modules);

            return data.ToString(Newtonsoft.Json.Formatting.None);
        }

        private void PutFolderData(String folder, JObject data)
        {
            IEnumerator<KeyValuePair<String, JToken>> enumer = data.GetEnumerator();

            foreach (String fileName in Directory.GetFiles(folder, "*.js"))
            {
                File.Delete(fileName);
            }

            while (enumer.MoveNext())
            {
                String fileName = enumer.Current.Key + ".js";
                String content = enumer.Current.Value.Value<String>();

                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(folder, fileName), false, Encoding.UTF8);
                sw.Write(content);
                sw.Close();
            }
        }

        private String GetAuthString(String email, String password)
        {
            String auth1 = email + ":" + password;

            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(auth1), Base64FormattingOptions.None);
        }

    }
}
