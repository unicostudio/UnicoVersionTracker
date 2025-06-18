using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnicoStudio.UnicoLibs.VersionTracker
{
    public static class UnicoVersionExporter
    {
        private const string ASSETS = "Assets";

        private static readonly JsonSerializerSettings s_jsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Formatting = Formatting.Indented,
        };

        private static readonly List<SdkInfo> s_sdkInfo = new()
        {
            new SdkInfo("UnicoAPIClient",
                new SdkVersionGetter(null, GetUnicoAPIClientVersion)),
            new SdkInfo("AppLovinMAX",
                new SdkVersionGetter("MaxSdk", GetAppLovinVersion, "AppLovinMax.Scripts.IntegrationManager.Editor.AppLovinIntegrationManager", GetAppLovinVersions)),
            new SdkInfo("GoogleAdMob",
                new SdkVersionGetter("GoogleMobileAds.Api.MobileAds", GetAdMobVersion, GetAdMobMediationVersions)),
            new SdkInfo("GoogleImmersiveAds",
                new SdkVersionGetter("GoogleMobileAds.Api.MobileAds", GetGoogleImmersiveAdsVersion)),
            new SdkInfo("Odeeo",
                new SdkVersionGetter("Odeeo.OdeeoSdk", GetOdeeoVersion)),
            new SdkInfo("AmazonSdk",
                new SdkVersionGetter("AmazonConstants", GetAmazonSdkVersion)),
            new SdkInfo("AdjustSdk",
                new SdkVersionGetter("AdjustSdk.Adjust", GetAdjustVersion)),
            new SdkInfo("FacebookSdk",
                new SdkVersionGetter("Facebook.Unity.FacebookSdkVersion", GetFacebookSdkVersion)),
            new SdkInfo("Firebase",
                new SdkVersionGetter("Firebase.FirebaseApp", GetFirebaseVersion, GetFirebaseVersions)),
        };

        /// <summary>
        /// Exports the build information to a json file.
        /// </summary>
        /// <param name="buildSummary">The build summary.</param>
        /// <remarks>
        /// The file path will be <c>Assets/../UnicoVersionTracker/[platform]_BuildInfo.json</c>.
        /// </remarks>
        public static async void ExportBuildInfoAsync(BuildSummary buildSummary)
        {
            try
            {
                UnicoVersionTrackerProgressBar.StartLoading();

                var buildInfo = new BuildInfo(buildSummary);
                var filePath = GetFilePath(string.Empty, $"{buildSummary.platform}_BuildInfo");
                var json = JsonConvert.SerializeObject(buildInfo, s_jsonSerializerSettings);

                // Save to file
                await File.WriteAllTextAsync(filePath, json);
                Debug.Log($"Build info saved to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error writing file: {ex}");
            }
            finally
            {
                UnicoVersionTrackerProgressBar.StopLoading();
            }
        }

        /// <summary>
        /// Exports the SDK information to a json file.
        /// </summary>
        /// <remarks>
        /// The file path will be <c>Assets/../UnicoVersionTracker/SdkInfo.json</c>.
        /// </remarks>
        [MenuItem("UnicoStudio/Export SdkInfo", priority = -1)]
        private static async void ExportSdkInfo()
        {
            try
            {
                UnicoVersionTrackerProgressBar.StartLoading();

                RefreshSdkInfo();
                var filePath = GetFilePath("SdkInfo", "SdkInfo");
                var json = JsonConvert.SerializeObject(s_sdkInfo, s_jsonSerializerSettings);

                // Save to file
                await File.WriteAllTextAsync(filePath, json);
                Debug.Log($"Sdk info saved to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error writing file: {ex}");
            }
            finally
            {
                UnicoVersionTrackerProgressBar.StopLoading();
            }
        }

        /// <summary>
        /// Asynchronously retrieves and deserializes the saved build information for the specified platform.
        /// </summary>
        /// <param name="platform">The target platform for which to retrieve the build information.</param>
        /// <returns>A <see cref="BuildInfo"/> object containing the saved build details, or null if an error occurs.</returns>
        public static async Task<BuildInfo> GetSavedBuildInfo(BuildTarget platform)
        {
            try
            {
                var json = await GetSavedBuildInfoJson(platform);
                var buildInfo = JsonConvert.DeserializeObject<BuildInfo>(json, s_jsonSerializerSettings);
                return buildInfo;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading file: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Asynchronously reads the saved build information JSON from a file for the specified platform.
        /// </summary>
        /// <param name="platform">The target platform for which to retrieve the JSON data.</param>
        /// <returns>A JSON string containing the build information, or null if an error occurs.</returns>
        public static async Task<string> GetSavedBuildInfoJson(BuildTarget platform)
        {
            try
            {
                var filePath = GetFilePath(string.Empty, $"{platform}_BuildInfo");
                var json = await File.ReadAllTextAsync(filePath);
                return json;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading file: {ex}");
                return null;
            }
        }

        private static string GetFilePath(string folderPathPostfix, string fileNamePostfix)
        {
            // Predefined folder path
            var folderPath = Path.Combine(ASSETS, "../UnicoVersionTracker/", folderPathPostfix);
            var fileName = $"{Application.productName}_{Application.version}_{fileNamePostfix}.json";
            var filePath = Path.Combine(folderPath, MakeFileNameFriendly(fileName));

            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return filePath;
        }

        private static string MakeFileNameFriendly(string fileName, bool removeSpaces = true)
        {
            var newName = Regex.Replace(fileName, $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "-");
            if (removeSpaces) newName = newName.Replace(" ", string.Empty);
            return newName;
        }

        /// <summary>
        /// Refreshes the SDK information by updating the version of each SDK.
        /// </summary>
        /// <remarks>
        /// Iterates through the list of SDKs and calls the <c>SetVersion</c> method 
        /// on each <c>SdkInfo</c> instance to update its version information.
        /// </remarks>
        private static void RefreshSdkInfo()
        {
            foreach (var sdkInfo in s_sdkInfo)
            {
                sdkInfo.SetVersion();
            }
        }

        /// <summary>
        /// Logs the given message as an error in the Unity console, with a red color.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private static void LogError(string message)
        {
            Debug.Log("<color=red>" + message + "</color>");
        }

        private static string GetAppLovinVersion(Type appLovinType)
        {
            if (appLovinType == null) return null;

            var property = appLovinType.GetProperty("Version", BindingFlags.Public | BindingFlags.Static);
            return property != null ? property.GetValue(null)?.ToString() : null;
        }

        private static List<VersionInfo> GetAppLovinVersions(Type appLovinType)
        {
            if (appLovinType == null) return null;

            // Get the LoadPluginData method
            var method = appLovinType.GetMethod("LoadPluginData", BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                LogError("LoadPluginData method not found!");
                return null;
            }

            var property = appLovinType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (property == null)
            {
                LogError("AppLovinIntegrationManager.Instance property not found!");
                return null;
            }

            var appLovinInstance = property.GetValue(null);
            if (appLovinInstance == null)
            {
                LogError("AppLovinIntegrationManager.Instance returned null!");
                return null;
            }

            // Use reflection to define a result variable dynamically
            object pluginData = null;

            // Create a callback action to capture the result (using reflection)
            Action<object> callback = data => { pluginData = data; };

            // Prepare parameters (callback passed as object)
            object[] parameters = { callback };

            // Invoke LoadPluginData and get the IEnumerator
            var enumerator = method.Invoke(appLovinInstance, parameters) as IEnumerator;
            if (enumerator == null)
            {
                LogError("LoadPluginData did not return IEnumerator!");
                return null;
            }

            // Process the enumerator until completion
            WaitForCompletion(enumerator);

            // If no result, return null
            if (pluginData == null)
            {
                LogError("LoadPluginData did not return any PluginData! You may have internet connection problem..");
                return null;
            }

            // Access the AppLovinMax field in PluginData
            var pluginDataType = pluginData.GetType();
            var appLovinMaxField = pluginDataType.GetField("AppLovinMax", BindingFlags.Public | BindingFlags.Instance);
            if (appLovinMaxField == null)
            {
                LogError("AppLovinMax field not found in PluginData!");
                return null;
            }

            var appLovinMax = appLovinMaxField.GetValue(pluginData);
            if (appLovinMax == null)
            {
                LogError("AppLovinMax is null!");
                return null;
            }

            var versionInfo = new List<VersionInfo>();

            // Access the MediatedNetworks field in PluginData
            var mediatedNetworksField = pluginDataType.GetField("MediatedNetworks", BindingFlags.Public | BindingFlags.Instance);
            if (mediatedNetworksField == null)
            {
                LogError("MediatedNetworks field not found in PluginData!");
                return null;
            }

            var mediatedNetworks = mediatedNetworksField.GetValue(pluginData) as object[];
            if (mediatedNetworks == null)
            {
                LogError("MediatedNetworks is null!");
                return null;
            }

            // Loop through MediatedNetworks and get the Unity version
            foreach (var network in mediatedNetworks)
            {
                if (network == null) continue;
                versionInfo.Add(GetVersionInfoForNetwork(network));
            }

            // Access the PartnerMicroSdks field in PluginData
            var partnerMicroSdksField = pluginDataType.GetField("PartnerMicroSdks", BindingFlags.Public | BindingFlags.Instance);
            if (partnerMicroSdksField == null)
            {
                LogError("MediatedNetworks field not found in PluginData!");
                return null;
            }

            var partnerMicroSdks = partnerMicroSdksField.GetValue(pluginData) as object[];
            if (partnerMicroSdks == null)
            {
                LogError("PartnerMicroSdks is null!");
                return null;
            }

            // Loop through MediatedNetworks and get the Unity version
            foreach (var network in partnerMicroSdks)
            {
                if (network == null) continue;
                versionInfo.Add(GetVersionInfoForNetwork(network));
            }

            return versionInfo;

            void WaitForCompletion(IEnumerator waitEnumerator)
            {
                // Process the enumerator synchronously
                while (waitEnumerator.MoveNext())
                {
                    // Handle yield return values if needed
                }
            }

            VersionInfo GetVersionInfoForNetwork(object networkObject)
            {
                var networkType = networkObject?.GetType();
                var name = networkType
                    ?.GetField("DisplayName", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(networkObject)
                    ?.ToString();

                var versionsField = networkType
                    ?.GetField("CurrentVersions", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(networkObject);

                var version = versionsField
                    ?.GetType().GetField("Unity", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(versionsField)
                    ?.ToString();

                return new VersionInfo(name, version);
            }
        }

        private static string GetAdMobVersion(Type _)
        {
            // First, find the GoogleMobileAds folder anywhere in the Assets folder
            var googleMobileAdsPath = Directory.GetDirectories(ASSETS, "*GoogleMobileAds", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(googleMobileAdsPath))
            {
                LogError("GoogleMobileAds folder not found!");
                return null;
            }

            var files = Directory.GetFiles(googleMobileAdsPath, "GoogleMobileAds_version*.txt");
            if (files.Length <= 0)
            {
                LogError("AdMob Unity version file not found!");
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(files[0]);

            // Extract version from filename (e.g., "GoogleMobileAds_version-9.1.0_manifest")
            var version = fileName.Split('-')[1].Replace("_manifest", string.Empty); // "9.1.0"
            return version;
        }

        private static List<VersionInfo> GetAdMobMediationVersions(Type _)
        {
            // First, find the GoogleMobileAds/Mediation folder anywhere in the Assets folder
            var mediationFolderPath = Directory.GetDirectories(ASSETS, "*GoogleMobileAds", SearchOption.AllDirectories)
                .Where(dir => Directory.Exists(Path.Combine(dir, "Mediation")))
                .Select(dir => Path.Combine(dir, "Mediation"))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(mediationFolderPath))
            {
                LogError("GoogleMobileAds/Mediation folder not found!");
                return null;
            }

            var versionInfo = new List<VersionInfo>();

            try
            {
                // Get all mediation adapter directories
                var adapterDirectories = Directory.GetDirectories(mediationFolderPath);

                foreach (var adapterDir in adapterDirectories)
                {
                    var adapterName = Path.GetFileName(adapterDir);
                    var editorPath = Path.Combine(adapterDir, "Editor");

                    if (!Directory.Exists(editorPath)) continue;

                    // Look for the mediation dependencies XML file
                    var dependenciesFiles = Directory.GetFiles(editorPath, "*MediationDependencies.xml");

                    if (dependenciesFiles.Length == 0)
                    {
                        Debug.Log($"AdMob mediation dependencies file not found for {adapterName} in: {editorPath}");
                        continue;
                    }

                    var dependenciesFile = dependenciesFiles[0]; // Use the first match

                    try
                    {
                        var xmlDocument = XDocument.Load(dependenciesFile);

                        // Extract Android version from androidPackage spec
                        var androidPackageNode = xmlDocument.Descendants("androidPackage").FirstOrDefault();
                        var spec = androidPackageNode?.Attribute("spec")?.Value;
                        string androidVersion = null;
                        if (!string.IsNullOrEmpty(spec))
                        {
                            // Extract version from spec like "com.google.ads.mediation:applovin:12.6.1.0"
                            var parts = spec.Split(':');
                            androidVersion = parts.Length >= 1 ? parts[^1] : null;
                        }

                        // Extract iOS version from iosPod version
                        var iosPodNode = xmlDocument.Descendants("iosPod").FirstOrDefault();
                        var iosVersion = iosPodNode?.Attribute("version")?.Value;

                        // Combine Android and iOS versions in the specified format
                        string combinedVersion = null;

                        if (!string.IsNullOrEmpty(androidVersion) && !string.IsNullOrEmpty(iosVersion))
                            combinedVersion = $"android_{androidVersion}_ios_{iosVersion}";
                        else if (!string.IsNullOrEmpty(androidVersion))
                            combinedVersion = $"android_{androidVersion}";
                        else if (!string.IsNullOrEmpty(iosVersion))
                            combinedVersion = $"ios_{iosVersion}";

                        if (!string.IsNullOrEmpty(combinedVersion))
                            versionInfo.Add(new VersionInfo(adapterName, combinedVersion));
                        else
                            LogError($"Failed to extract version for AdMob {adapterName} mediation adapter!");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error parsing mediation dependencies for {adapterName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error reading AdMob mediation directories: {ex.Message}");
                return null;
            }

            return versionInfo.Any() ? versionInfo : null;
        }

        private static string GetGoogleImmersiveAdsVersion(Type _)
        {
            // First, find the GoogleMobileAdsNative/Editor folder anywhere in the Assets folder
            var googleMobileAdsNativeEditorPath = Directory.GetDirectories(ASSETS, "*GoogleMobileAdsNative", SearchOption.AllDirectories)
                .Where(dir => Directory.Exists(Path.Combine(dir, "Editor")))
                .Select(dir => Path.Combine(dir, "Editor"))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(googleMobileAdsNativeEditorPath))
            {
                LogError("GoogleMobileAdsNative/Editor folder not found!");
                return null;
            }

            var dependenciesPath = Path.Combine(googleMobileAdsNativeEditorPath, "GoogleMobileAdsNativeDependencies.xml");
            if (!File.Exists(dependenciesPath))
            {
                LogError("Google immersive ads dependencies file not found!");
                return null;
            }

            var xmlDocument = XDocument.Load(dependenciesPath);
            var androidPackageNode = xmlDocument.Descendants("androidPackage")
                .FirstOrDefault(node => node.Attribute("spec")?.Value.Contains("gson") == true);

            if (androidPackageNode != null)
            {
                var spec = androidPackageNode.Attribute("spec")?.Value;
                if (!string.IsNullOrEmpty(spec))
                {
                    var parts = spec.Split(':');
                    if (parts.Length >= 1)
                    {
                        return parts[^1];
                    }
                }
            }

            LogError("Failed to fetch Google immersive ads version!");
            return null;
        }

        private static string GetOdeeoVersion(Type odeeoType)
        {
            if (odeeoType == null) return null;

            var field = odeeoType.GetField("SDK_VERSION", BindingFlags.Public | BindingFlags.Static);
            var sdkVersion = field?.GetValue(null)?.ToString();
            if (sdkVersion == null)
            {
                LogError("SDK_VERSION field not found in OdeeoSdk!");
                return null;
            }

            var match = Regex.Match(sdkVersion, @"v(\d+\.\d+\.\d+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string GetAmazonSdkVersion(Type amazonType)
        {
            if (amazonType == null) return null;

            var field = amazonType.GetField("VERSION", BindingFlags.Public | BindingFlags.Static);
            return field != null ? field.GetValue(null)?.ToString() : null;
        }

        private static string GetAdjustVersion(Type _)
        {
            // First, find the Adjust folder anywhere in the Assets folder
            var adjustPath = Directory.GetDirectories(ASSETS, "*Adjust", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(adjustPath))
            {
                LogError("Adjust folder not found!");
                return null;
            }

            var packageJsonPath = Path.Combine(adjustPath, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                LogError("Adjust package.json not found!");
                return null;
            }

            var jsonContent = File.ReadAllText(packageJsonPath);
            var jsonObject = JObject.Parse(jsonContent);

            // Extract the "version" field
            var version = jsonObject["version"]?.ToString();
            if (string.IsNullOrEmpty(version))
            {
                LogError("Adjust version not found in package.json!");
                return null;
            }

            return version;
        }

        private static string GetFacebookSdkVersion(Type facebookType)
        {
            if (facebookType == null) return null;

            var property = facebookType.GetProperty("Build", BindingFlags.Public | BindingFlags.Static);
            return property != null ? property.GetValue(null)?.ToString() : null;
        }

        private static string GetFirebaseVersion(Type _)
        {
            // First, find the Firebase/Editor folder anywhere in the Assets folder
            var firebaseEditorPath = Directory.GetDirectories(ASSETS, "*Firebase", SearchOption.AllDirectories)
                .Where(dir => Directory.Exists(Path.Combine(dir, "Editor")))
                .Select(dir => Path.Combine(dir, "Editor"))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(firebaseEditorPath))
            {
                LogError("Firebase/Editor folder not found!");
                return null;
            }

            var dependenciesPath = Path.Combine(firebaseEditorPath, "AppDependencies.xml");
            if (!File.Exists(dependenciesPath))
            {
                LogError("Firebase AppDependencies.xml file not found!");
                return null;
            }

            // Load the XML document
            var xmlDocument = XDocument.Load(dependenciesPath);

            // Find the <androidPackage> node with 'unity' in its spec attribute
            var androidPackageNode = xmlDocument.Descendants("androidPackage")
                .FirstOrDefault(node => node.Attribute("spec")?.Value.Contains("unity") == true);

            if (androidPackageNode != null)
            {
                // Extract the spec attribute value
                var spec = androidPackageNode.Attribute("spec")?.Value;
                if (!string.IsNullOrEmpty(spec))
                {
                    // Split the spec to get the version
                    var parts = spec.Split(':');
                    if (parts.Length >= 1)
                    {
                        return parts[^1]; // return the version part
                    }
                }
            }

            LogError("Failed to fetch Firebase version!");
            return null;
        }

        private static List<VersionInfo> GetFirebaseVersions(Type _)
        {
            // First, find the Firebase/Editor folder anywhere in the Assets folder
            var firebaseEditorPath = Directory.GetDirectories(ASSETS, "*Firebase", SearchOption.AllDirectories)
                .Where(dir => Directory.Exists(Path.Combine(dir, "Editor")))
                .Select(dir => Path.Combine(dir, "Editor"))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(firebaseEditorPath))
            {
                LogError("Firebase/Editor folder not found!");
                return null;
            }

            var files = Directory.GetFiles(firebaseEditorPath, "Firebase*_version*.txt");
            if (files.Length <= 0)
            {
                LogError("Firebase plugin version files not found!");
                return null;
            }

            var versionInfo = new List<VersionInfo>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var split = fileName.Split('-');

                var name = split[0].Replace("_version", string.Empty); // "FirebaseAnalytics"
                var version = split[1].Replace("_manifest", string.Empty); // "12.1.0"
                versionInfo.Add(new VersionInfo(name, version));
            }

            return versionInfo;
        }

        private static string GetUnicoAPIClientVersion(Type _)
        {
            try
            {
                var packagesConfigPath = Path.Combine(ASSETS, "packages.config");
                if (!File.Exists(packagesConfigPath))
                {
                    LogError("packages.config not found!");
                    return null;
                }

                var xmlDocument = XDocument.Load(packagesConfigPath);
                var unicoApiClientPackage = xmlDocument.Descendants("package")
                    .FirstOrDefault(node => node.Attribute("id")?.Value == "unicoapiclient");

                if (unicoApiClientPackage != null)
                {
                    return unicoApiClientPackage.Attribute("version")?.Value;
                }

                LogError("UnicoAPIClient package not found in packages.config!");
                return null;
            }
            catch (Exception ex)
            {
                LogError($"Failed to get UnicoAPIClient version: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Finds the type with the given <paramref name="typeFullName"/> in the loaded assemblies.
        /// </summary>
        /// <param name="typeFullName">The name of the type to search for.</param>
        /// <returns>The found type, or <c>null</c> if the type is not found.</returns>
        private static Type FindTypeInAssemblies(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName)) return null;

            // Search through all loaded assemblies
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeFullName))
                .FirstOrDefault(type => type != null);

            if (type == null)
            {
                LogError($"Type not found in the project: {typeFullName}");
            }

            return type;
        }

        public record BuildInfo
        {
            public ProjectInfo ProjectInfo { get; }
            public List<SdkInfo> SdkInfo { get; }

            [JsonConstructor]
            public BuildInfo(ProjectInfo projectInfo, List<SdkInfo> sdkInfo)
            {
                ProjectInfo = projectInfo;
                SdkInfo = sdkInfo;
            }

            public BuildInfo(BuildSummary buildSummary)
            {
                ProjectInfo = new ProjectInfo(buildSummary);
                SdkInfo = s_sdkInfo;
                RefreshSdkInfo();
            }
        }

        public record ProjectInfo
        {
            public string Platform { get; }
            public string UnityVersion { get; }
            public string PackageName { get; }
            public string Version { get; }
            public string CompressionMethod { get; }
            public List<string> GraphicsAPIs { get; }
            public string ManagedStrippingLevel { get; }
            public string RenderPipeline { get; }
            public AndroidInfo Android { get; }
            public IOSInfo IOS { get; }

            [JsonConstructor]
            public ProjectInfo(string platform,
                string unityVersion,
                string packageName,
                string version,
                string compressionMethod,
                List<string> graphicsAPIs,
                string managedStrippingLevel,
                string renderPipeline,
                AndroidInfo android,
                IOSInfo ios)
            {
                Platform = platform;
                UnityVersion = unityVersion;
                PackageName = packageName;
                Version = version;
                CompressionMethod = compressionMethod;
                GraphicsAPIs = graphicsAPIs;
                ManagedStrippingLevel = managedStrippingLevel;
                RenderPipeline = renderPipeline;
                Android = android;
                IOS = ios;
            }

            public ProjectInfo(BuildSummary buildSummary)
            {
                Platform = buildSummary.platform.ToString();
                UnityVersion = Application.unityVersion;
                PackageName = Application.identifier;
                Version = PlayerSettings.bundleVersion;
                CompressionMethod = GetCompressionMethod(buildSummary.options);
                GraphicsAPIs = GetGraphicsAPI(buildSummary.platform);
                ManagedStrippingLevel = GetManagedStrippingLevel(buildSummary.platformGroup);
                RenderPipeline = GetRenderPipeline();

                if (buildSummary.platform == BuildTarget.Android) Android = new AndroidInfo();
                if (buildSummary.platform == BuildTarget.iOS) IOS = new IOSInfo();
            }

            public record AndroidInfo
            {
                public int BundleVersionCode { get; }
                public int MinSdkVersion { get; }
                public int TargetSdkVersion { get; }

                [JsonConstructor]
                public AndroidInfo(int bundleVersionCode, int minSdkVersion, int targetSdkVersion)
                {
                    BundleVersionCode = bundleVersionCode;
                    MinSdkVersion = minSdkVersion;
                    TargetSdkVersion = targetSdkVersion;
                }

                public AndroidInfo()
                {
                    BundleVersionCode = PlayerSettings.Android.bundleVersionCode;
                    MinSdkVersion = (int)PlayerSettings.Android.minSdkVersion;
                    TargetSdkVersion = (int)PlayerSettings.Android.targetSdkVersion;
                }
            }

            public record IOSInfo
            {
                public int BuildNumber { get; }
                public string TargetOSVersion { get; }

                [JsonConstructor]
                public IOSInfo(int buildNumber, string targetOSVersion)
                {
                    BuildNumber = buildNumber;
                    TargetOSVersion = targetOSVersion;
                }

                public IOSInfo()
                {
                    BuildNumber = int.Parse(PlayerSettings.iOS.buildNumber);
                    TargetOSVersion = PlayerSettings.iOS.targetOSVersionString;
                }
            }

            private static string GetCompressionMethod(BuildOptions buildOptions)
            {
                return buildOptions.HasFlag(BuildOptions.CompressWithLz4) ? "LZ4" :
                    buildOptions.HasFlag(BuildOptions.CompressWithLz4HC) ? "LZ4HC" : "Default";
            }

            private static List<string> GetGraphicsAPI(BuildTarget target)
            {
                return PlayerSettings.GetGraphicsAPIs(target).Select(graphicsAPI => graphicsAPI.ToString()).ToList();
            }

            private static string GetManagedStrippingLevel(BuildTargetGroup buildTargetGroup)
            {
                return PlayerSettings.GetManagedStrippingLevel(buildTargetGroup).ToString();
            }

            private static string GetRenderPipeline()
            {
                try
                {
                    // Get the current render pipeline asset from Graphics Settings
                    var renderPipelineAsset = GraphicsSettings.currentRenderPipeline;
                    if (!renderPipelineAsset) return "Built-in";

                    // Get the type name of the render pipeline asset
                    var typeName = renderPipelineAsset.GetType().Name;

                    return typeName switch
                    {
                        "UniversalRenderPipelineAsset" => "URP",
                        "HDRenderPipelineAsset" => "HDRP",
                        _ => $"Custom ({typeName})"
                    };
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to determine render pipeline: {ex.Message}");
                    return "Unknown";
                }
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public record SdkInfo
        {
            [JsonProperty] public string Name { get; private set; }
            [JsonProperty] public string Version { get; private set; }
            [JsonProperty] public List<VersionInfo> PluginVersionInfo { get; private set; }
            private SdkVersionGetter VersionGetter { get; }

            [JsonConstructor]
            public SdkInfo(string name, string version)
            {
                Name = name;
                Version = version;
            }

            public SdkInfo(string name, SdkVersionGetter versionGetter)
            {
                Name = name;
                VersionGetter = versionGetter;
            }

            public void SetVersion()
            {
                if (VersionGetter == null) return;

                var type1 = FindTypeInAssemblies(VersionGetter.TypeFullName1);
                var type2 = FindTypeInAssemblies(VersionGetter.TypeFullName2);

                Version = VersionGetter.Getter1?.Invoke(type1);
                PluginVersionInfo = VersionGetter.Getter2?.Invoke(type2);
            }
        }

        public record SdkVersionGetter(
            string TypeFullName1,
            Func<Type, string> Getter1,
            string TypeFullName2 = null,
            Func<Type, List<VersionInfo>> Getter2 = null)
        {
            public string TypeFullName1 { get; } = TypeFullName1;
            public Func<Type, string> Getter1 { get; } = Getter1;
            public string TypeFullName2 { get; } = TypeFullName2;
            public Func<Type, List<VersionInfo>> Getter2 { get; } = Getter2;

            public SdkVersionGetter(
                string typeFullName,
                Func<Type, string> getter1,
                Func<Type, List<VersionInfo>> getter2) : this(typeFullName, getter1, typeFullName, getter2)
            {
            }

            public SdkVersionGetter(
                string typeFullName,
                Func<Type, List<VersionInfo>> getter2) : this(typeFullName, null, typeFullName, getter2)
            {
            }
        }

        public record VersionInfo(string Name, string Version)
        {
            public string Name { get; private set; } = Name;
            public string Version { get; private set; } = Version;
        }
    }
}