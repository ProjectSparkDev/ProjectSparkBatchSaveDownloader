using Newtonsoft.Json;
using System.Reflection;
using XboxCsMgr.Helpers.Win32;
using XboxCsMgr.XboxLive;
using XboxCsMgr.XboxLive.Exceptions;
using XboxCsMgr.XboxLive.Model.Authentication;
using XboxCsMgr.XboxLive.Model.TitleStorage;
using XboxCsMgr.XboxLive.Services;



namespace PSBSD
{
    internal class Config
    {
        internal static readonly string ServiceId = "d3d00100-7976-472f-a3f7-bc1760d19e14";
        public static readonly string FinalMessage = "Thank you for using our app.\n\nfor the sake of preservation please consider sharing/donating your worlds and downloaded worlds with us at sparkdev discord\nwe are trying to launch a preservation and archival project.\nclick on the discord icon for a invite.";
        internal static readonly string Disclaimer = "DISCLAIMER:\n* I understand that this software connects to Xbox live REST API and beside that no data is collected or shared.\n* I understand that the credentials are taken from the Xbox app and login details are not necesary\n* and i understand that running this program multiple times a in an hour has the chance to get me rate limited and potentially banned.\n* i understand that developer is not responsible for my misuse";
        internal static readonly string PackageFamilyName = "Microsoft.ProjectSpark-Dakota_8wekyb3d8bbwe";
        internal static readonly string MetaFileName = ".partial.spark";
        internal static readonly DateTime LaunchDatetime = DateTime.Now;
        internal static string UserToken { get; set; }
        internal static string DeviceToken { get; set; }
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
                    if (File.Exists(Path.Combine(dialog.SelectedPath, Config.MetaFileName)))
                    {
                        Log("partial download detected. Folder selected");
                    }
                    else
                    {
                        _ = MessageBox.Show("Please select a Empty folder", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SelectFolder();
                        return;
                    }
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
        internal static Dictionary<string, string> LoadXblTokenCredentials()
        {
            Dictionary<string, string> currentCredentials = CredentialUtil.EnumerateCredentials();
            Log($"loaded total of {currentCredentials.Count} credentials");
            Dictionary<string, string> xblCredentials = currentCredentials.Where(k => k.Key.Contains("Xbl|") || (k.Key.Contains("XblGrts|")
                    && k.Key.Contains("Dtoken"))
                    || k.Key.Contains("Utoken"))
                    .ToDictionary(p => p.Key, p => p.Value);

            Log($"filtered {xblCredentials.Count} credentials with the xbox live tags");

            Dictionary<string, string> fulltokens = [];

            foreach (KeyValuePair<string, string> cred in xblCredentials)
            {
                try
                {
                    string[] Properties = cred.Key.Split('|');
                    //look for headers or ignore
                    if (Properties.Length == 9 && Properties.Last() == "JWT")//token is header
                    {
                        string headerKey = cred.Key;
                        string headerValue = cred.Value;
                        Dictionary<string, string> partials = [];
                        foreach (KeyValuePair<string, string> others in xblCredentials.Where(k => k.Key != cred.Key))
                        {
                            string[] properties = others.Key.Split('|');
                            if (properties[1] == Properties[1] &&
                                properties[2] == Properties[2] &&
                                properties[5] == Properties[5] &&
                                properties.Length == 10 &&
                                properties.Last() != "JWT")
                            { //partial of the same header
                                partials.Add(others.Key, others.Value);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (partials.Count > 0)
                        {
                            if (partials.Count == 1)
                            {
                                //merge tokens
                                headerValue = $"{headerValue}{partials.First().Value}";
                            }
                            else
                            {
                                Dictionary<int, string> indexes = [];
                                foreach (KeyValuePair<string, string> partial in partials)
                                {
                                    indexes.Add(Convert.ToInt32(partial.Key.Split('|').Last()), partial.Key);
                                }
                                for (int i = 0; i < indexes.Count; i++)
                                {
                                    headerValue = $"{headerValue}{partials[indexes[i]]}";
                                }
                            }
                        }
                        fulltokens.Add(headerKey, headerValue);
                    }
                    else
                    { //ignore
                        continue;
                    }

                }
                catch (IndexOutOfRangeException)
                {
                    Log($"unsupported Key format: {cred.Key}");
                }
                catch (Exception e)
                {
                    Error(e);
                    return null;
                }
            }
            Log($"merged tokens successfully. available tokens:({fulltokens.Count})");
            return fulltokens;
        }
        internal static void UpdateCredentials()
        {
            Dictionary<string, string> Credentials = LoadXblTokenCredentials();
            List<XboxLiveToken> tokens = [];
            Log("using the first available user. support for multiple users will be added later.");
            KeyValuePair<string, string> first = Credentials.First();
            foreach (KeyValuePair<string, string> key in Credentials)
            {
                Log($"valid token id found: ({key.Key.Split('|')[1]})");
                if (key.Key.Split('|')[1] == first.Key.Split('|')[1])
                {
                    tokens.Add(JsonConvert.DeserializeObject<XboxLiveToken>(key.Value));
                }
            }
            foreach (XboxLiveToken token in tokens)
            {
                if (token.IdentityType == "Dtoken")
                {
                    Config.DeviceToken = token.TokenData.Token;
                    Log($"Token set. type:({token.TokenType}) - Identity:({token.IdentityType})");
                }
                else if (token.IdentityType == "Utoken")
                {
                    Config.UserToken = token.TokenData.Token;
                    Log($"Token set. type:({token.TokenType}) - Identity:({token.IdentityType})");
                }
                else
                {
                    Error("invalid token type was used. wrong implementation");
                    return;
                }
            }

        }
        internal static async Task AuthenticateXbl()
        {
            XboxLiveAuthenticateResponse<XboxLiveDisplayClaims> result;

            result = await authenticateService.AuthorizeXsts(Config.UserToken, Config.DeviceToken);

            if (result != null)
            {
                Log($"Authorized as :{result.DisplayClaims.XboxUserIdentity.First().Gamertag}");
                XblConfig = new XboxLiveConfig(result.Token, result.DisplayClaims.XboxUserIdentity[0]);
                authenticateService = new AuthenticateService(XblConfig);
                _storageService = new TitleStorageService(XblConfig, Config.PackageFamilyName, Config.ServiceId);
            }
            return;
        }
        internal static async Task<byte[]> DownloadAtomData(TitleStorageBlobMetadata b)
        {
            TitleStorageAtomMetadataResult downloadAtom = await _storageService.GetBlobAtoms(b.FileName);
            return await _storageService.DownloadAtomAsync(downloadAtom.Atoms["Data"]);

        }
        internal static async Task Download()
        {
            bool noerror = true;
            bool partial = false;
            Downloader.main.ProgressBarMarquee();
            Downloader.main.Disableinput();
            if (Directory.EnumerateFileSystemEntries(Config.OutputPath).Any())
            {
                if (File.Exists(Path.Combine(Config.OutputPath, Config.MetaFileName)))
                {
                    partial = true;
                }
                else
                {
                    Error("FOLDER CHOSEN IS NOT EMPTY\nno previous download was detected. so please select another folder.");
                    return;
                }
            }

            if (!Directory.Exists(Config.OutputPath))
            {
                Error("FOLDER DOES NOT EXIST\nwrong folder selected. please try again.");
                return;
            }


            Log("Loading Xbox live credentials");
            try
            {
                UpdateCredentials();
            }
            catch (Exception E)
            {
                Error(E);
                return;
            }

            if (Config.DeviceToken == null || Config.UserToken == null || (Config.UserToken == Config.DeviceToken))
            {
                Error(" No credentials were found.\nmake sure to log into the xbox app at least once.");
                return;
            }

            Log("Authenticating...");
            try
            {
                await AuthenticateXbl();
            }
            catch (Exception E)
            {
                Error(E);
                return;
            }
            Log("Fetching save data...");
            IList<TitleStorageBlobMetadata> _saveData;
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
                return;
            }

            Log($"Found {_saveData.Count} blobs");
            Downloader.main.ProgressBarUpdate(0, _saveData.Count);
            Log($"Saving in the output folder: {Config.OutputPath}");

            Log("Preparing for download");
            //creating meta files for partial download support
            if (!partial)
            {
                using FileStream fs = new(Path.Combine(Config.OutputPath, Config.MetaFileName), FileMode.CreateNew);
                using StreamWriter sw = new(fs);
                await sw.WriteLineAsync("file to support partial downloads using spark save downloader");
            }

            foreach (TitleStorageBlobMetadata blob in _saveData)
            {


                string filename = blob.FileName.Split('/').Last();
                filename = filename.Replace(",savedgame", "");
                filename = filename.Replace("X", ".");
                filename = filename.Replace("E", "_");
                string localpath = string.Join(Path.DirectorySeparatorChar, blob.FileName.Split('/').SkipLast(1));
                localpath = localpath.Replace("X", ".");
                localpath = localpath.Replace("E", "_");
                string fullpath = Path.Combine(Config.OutputPath, localpath);
                _ = Directory.CreateDirectory(fullpath);
                string FilePath = Path.Combine(fullpath, filename);

                //ignore txt files
                if (Path.GetExtension(filename) == ".txt")
                {
                    Log("Downloading txt files are currently not supported. skipping");
                    continue;
                }

                if (partial)
                {
                    if (File.Exists(FilePath))
                    {
                        if (new FileInfo(FilePath).Length != 0)
                        {
                            Log($"skipping {filename} already exists");
                            continue;
                        }
                        else
                        {
                            try
                            {
                                File.Delete(FilePath);
                            }
                            catch (Exception ex)
                            {
                                Error(ex);
                                noerror = false;
                                Log($"skipping {filename} Couldnt delete the invalid file");
                                continue;
                            }
                        }
                    }
                }

                Log($"Downloading Atom: {filename}");

                byte[] atomData = null;
                try
                {
                    atomData = await DownloadAtomData(blob);
                }
                catch (Exception e)
                {
                    Log($"exception encountered when downloading ({FilePath}) - ({blob.FileName})");
                    Error(e);
                    noerror = false;

                    DialogResult res = MessageBox.Show($"downloading {filename} failed.\nretry?\n\nyes: retry downloading the same file\nno: skip the file\ncancel: cancel all download", "Download error", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                    if (res == DialogResult.Yes)
                    {
                        bool retry = true;
                        bool first = true;
                        while (retry)
                        {
                            DialogResult again = DialogResult.Yes;
                            if (!first)
                            {
                                again = MessageBox.Show($"downloading {filename} failed again. retry?", "Download error", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            }
                            first = false;
                            if (again == DialogResult.Yes)
                            {
                                Log("Loading Xbox live credentials");
                                try
                                {
                                    UpdateCredentials();
                                }
                                catch (Exception E)
                                {
                                    Error(E);
                                    continue;
                                }

                                if (Config.DeviceToken == null || Config.UserToken == null || (Config.UserToken == Config.DeviceToken))
                                {
                                    Error(" No credentials were found.\nmake sure to log into the xbox app at least once.");
                                    continue;
                                }

                                Log("Authenticating...");
                                try
                                {
                                    await AuthenticateXbl();
                                }
                                catch (Exception E)
                                {
                                    Error(E);
                                    continue;
                                }
                                try
                                {
                                    Log($"downloading Atom: {filename}");
                                    atomData = await DownloadAtomData(blob);
                                }
                                catch (Exception a)
                                {
                                    Error(a);
                                    continue;
                                }
                            }
                            else
                            {
                                retry = false;
                            }
                        }
                    }
                    else if (res == DialogResult.No)
                    {
                        continue;
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        Log("Canceling download");
                        return;
                    }
                }

                try
                {
                    //saving
                    using FileStream fs = new(FilePath, FileMode.CreateNew);
                    await fs.WriteAsync(atomData);
                }
                catch (Exception e)
                {
                    noerror = false;
                    Error(e);
                }
                Downloader.main.ProgressBarUpdate();

            }

            //cleaning up
            try
            {
                Log("cleaning up");
                if (noerror)
                {
                    File.Delete(Path.Combine(Config.OutputPath, Config.MetaFileName));
                }
            }
            catch (Exception e)
            {
                Error(e);
            }
            Log("FINISHED - ENJOY SPARKING!");
            _ = MessageBox.Show(Config.FinalMessage, "thank you");
            return;
        }
    }
}
