using Newtonsoft.Json;
using System.Reflection;
using XboxCsMgr.Helpers.Win32;
using XboxCsMgr.XboxLive;
using XboxCsMgr.XboxLive.Model.Authentication;
using XboxCsMgr.XboxLive.Model.TitleStorage;
using XboxCsMgr.XboxLive.Services;



namespace PSBSD
{
    internal class Config
    {
        internal static readonly string ServiceId = "d3d00100-7976-472f-a3f7-bc1760d19e14";
        internal static readonly string PackageFamilyName = "Microsoft.ProjectSpark-Dakota_8wekyb3d8bbwe";
        internal static string UserToken { get; set; }
        internal static string DeviceToken { get; set; }
        internal static readonly DateTime LaunchDatetime = DateTime.Now;
        internal static readonly string Disclaimer = "DISCLAIME:\n-I understand that this software connects to Xbox live REST API\n-I understand that the credentials are taken from the Xbox app\n-and i understand that running this program multiple times a day can get me rate limited and potentially banned";
        internal static string OutputPath = "";
    }
    internal class Tools
    {
        internal static XboxLiveConfig XblConfig { get; set; }
        private static TitleStorageService _storageService;
        private static AuthenticateService authenticateService = new(XblConfig);

        public static void SelectFolder()
        {
            FolderBrowserDialog dialog = new();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                
                if (Directory.EnumerateFileSystemEntries(dialog.SelectedPath).Any())
                {
                    _ = MessageBox.Show("Please select a Empty folder", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SelectFolder();
                    return;
                }
                Config.OutputPath = dialog.SelectedPath;
                Tools.Log($"Output Folder selected: {dialog.SelectedPath}");
            }
            else
            {
                return;
            }
        }

        public static void Error(Exception ex)
        {
            Log($"Error:{ex.GetType()}\n\n{ex.Message}\n\n{ex.StackTrace}");
            _ = MessageBox.Show($"Error:{ex.GetType()}\n\n{ex.Message}\n\n{ex.StackTrace}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log("Exiting");
            Environment.Exit(1);
        }
        public static void Error(string s)
        {
            Log($"Error:{s}");
            _ = MessageBox.Show($"Error:{s}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void Log(string s)
        {
            using (FileStream output = new(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Config.LaunchDatetime.ToFileTime() + ".log", FileMode.Append))
            using (StreamWriter sw = new(output))
            {
                sw.Write($"({DateTime.Now.ToLongTimeString()}) - ");
                sw.WriteLine(s);
            }
            Downloader.main.Log(s);
        }
        internal static void LoadXblTokenCredentials()
        {
            Dictionary<string, string> currentCredentials = CredentialUtil.EnumerateCredentials();
            Dictionary<string, string> xblCredentials = currentCredentials.Where(k => k.Key.Contains("Xbl|") || (k.Key.Contains("XblGrts|")
                    && k.Key.Contains("Dtoken"))
                    || k.Key.Contains("Utoken"))
                    .ToDictionary(p => p.Key, p => p.Value);

            string PartialCredential = null;
            foreach (KeyValuePair<string, string> credential in xblCredentials)
            {
                string json = credential.Value;
                XboxLiveToken token = null;
                try
                {
                    token = JsonConvert.DeserializeObject<XboxLiveToken>(json);
                }
                catch (JsonReaderException)
                {

                    if (PartialCredential == null)
                    {
                        PartialCredential = json;
                    }
                    else
                    {
                        try
                        {
                            token = JsonConvert.DeserializeObject<XboxLiveToken>(json + PartialCredential);
                        }
                        catch (JsonReaderException)
                        {
                            try
                            {
                                token = JsonConvert.DeserializeObject<XboxLiveToken>(PartialCredential + json);
                            }
                            catch (JsonReaderException)
                            {
                                PartialCredential = json;
                                break;
                            }
                        }
                    }
                }
                if (token != null)
                {
                    if (token.TokenData.NotAfter > DateTime.UtcNow)
                    {
                        if (credential.Key.Contains("Dtoken"))
                        {
                            Config.DeviceToken = token.TokenData.Token;
                        }
                        else if (credential.Key.Contains("Utoken"))
                        {
                            if (token.TokenData.Token != "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
                            {
                                Config.UserToken = token.TokenData.Token;
                            }
                        }
                    }
                }
            }
        }
        internal static async Task AuthenticateXbl()
        {
            XboxLiveAuthenticateResponse<XboxLiveDisplayClaims> result;

            try
            {
                result = await authenticateService.AuthorizeXsts(Config.UserToken, Config.DeviceToken);
            }
            catch (Exception ex)
            {
                Error(ex);
                Environment.Exit(1);
                return;
            }
            if (result != null)
            {
                Log($"Authorized as :{result.DisplayClaims.XboxUserIdentity.First().Gamertag}");
                XblConfig = new XboxLiveConfig(result.Token, result.DisplayClaims.XboxUserIdentity[0]);
                authenticateService = new AuthenticateService(XblConfig);
                _storageService = new TitleStorageService(XblConfig, Config.PackageFamilyName, Config.ServiceId);
            }
            return;
        }
        internal static async Task Download()
        {
            Downloader.main.ProgressBarMarquee();
            Downloader.main.Disableinput();
            if (Directory.EnumerateFileSystemEntries(Config.OutputPath).Any())
            {
                Error(new Exception("FOLDER CHOSEN IS NOT EMPTY"));
            }

            if (!Directory.Exists(Config.OutputPath))
            {
                Error(new Exception("FOLDER DOES NOT EXIST"));
            }

            IList<TitleStorageBlobMetadata> _saveData = [];

            Log("Loading Xbox live credentials");
            LoadXblTokenCredentials();

            if (Config.DeviceToken == null || Config.UserToken == null)
            {
                Error(new Exception("TOKENS WERE NULL"));
            }

            Log("Authenticating...");
            await AuthenticateXbl();
            Log("Fetching save data...");


            try
            {
                TitleStorageBlobMetadataResult blobMetadataResult = await _storageService.GetBlobMetadata();
                if (blobMetadataResult.pagingInfo.continuationToken != null)
                {
                    throw new Exception("Final token is not null, wrong implementation");
                }
                _saveData = blobMetadataResult.Blobs;

            }
            catch (Exception e)
            {
                Error(e);
            }

            Log($"Found {_saveData.Count} blobs");
            Downloader.main.ProgressBarUpdate(0, _saveData.Count);
            Log($"Saving in the output folder: {Config.OutputPath}");

            foreach (TitleStorageBlobMetadata blob in _saveData)
            {

                string filename = blob.FileName.Split('/').Last();
                filename = filename.Replace(",savedgame", "");
                filename = filename.Replace("X", ".");
                filename = filename.Replace("E", "_");
                Log($"Downloading Atom: {filename}");
                string localpath = string.Join(Path.DirectorySeparatorChar, blob.FileName.Split('/').SkipLast(1));
                string fullpath = Path.Combine(Config.OutputPath, localpath);
                _ = Directory.CreateDirectory(fullpath);
                string FilePath = Path.Combine(fullpath, filename);

                //downloading
                TitleStorageAtomMetadataResult downloadAtom = await _storageService.GetBlobAtoms(blob.FileName);
                byte[] atomData = await _storageService.DownloadAtomAsync(downloadAtom.Atoms["Data"]);

                //saving
                using (FileStream fs = new(FilePath, FileMode.CreateNew))
                {
                    await fs.WriteAsync(atomData);
                }
                Downloader.main.ProgressBarUpdate();

            }
            Log("FINISHED - ENJOY SPARKING!");
            Downloader.main.Enableinput();
            return;
        }


    }
}
