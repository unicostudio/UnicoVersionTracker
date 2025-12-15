# Changelog

All notable changes to this package will be documented in this file.

## [1.4.0] - 2025-12-15

* **NEW FEATURE**: Added UnicoConfig GameId integration to build information
  * Automatically retrieves GameId from UnicoConfig.Instance using reflection
  * Added `GameId` property to `ProjectInfo` class
  * Included in all exported build info JSON files

* **NEW FEATURE**: Added AdManagerSettings mediation type detection
  * Automatically detects Android and iOS mediation types from AdManagerSettings
  * Added `MediationTypes` property to `ProjectInfo` class
  * Reports configured mediation types (MaxAndAdmob, AdmobMediation, etc.)

* **NEW FEATURE**: Added Google ODM (On-Device Mediation) version tracking
  * Detects AdjustGoogleOdm and GoogleAdsOnDeviceConversion versions from XML
  * Added as separate SDK entry in sdkInfo list
  * iOS-only feature with proper platform detection
  * Extracts versions from AdjustGoogleODMDependencies.xml

* **NEW FEATURE**: Enhanced network version tracking with detailed metadata
  * Added unique network IDs for all mediation networks (e.g., `_applovin_`, `_facebook_`)
  * Added separate Android and iOS version fields for each network
  * Network IDs are fixed in code to prevent changes when network names change
  * Comprehensive ID mapping for 50+ networks (AppLovin MAX, AdMob, Firebase)
  * Combined version format: "android_X.X.X_ios_Y.Y.Y"

* **NEW FEATURE**: Added Unity Editor test build info export
  * New menu item: "UnicoStudio/Export Test BuildInfo"
  * Allows generating build info without actual build process
  * Uses current active build target from Editor settings
  * Useful for testing and verification purposes

* **IMPROVEMENT**: Restructured JSON output for better organization
  * Moved `MediationTypes` from root level into `ProjectInfo`
  * Converted `GoogleOdm` from separate block to SDK entry in `sdkInfo` list
  * More consistent and hierarchical JSON structure

* **IMPROVEMENT**: Updated deprecated Unity API usage
  * Replaced `PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup)` with `NamedBuildTarget` version
  * Added `UnityEditor.Build` namespace import
  * Ensures compatibility with latest Unity versions

* **CODE QUALITY**: Enhanced reflection-based SDK detection
  * All SDK version retrieval uses consistent reflection patterns
  * Network ID mapping centralized in dictionary for easy maintenance
  * Improved error handling and logging for SDK detection failures

## [1.3.0] - 2025-06-18

* **NEW FEATURE**: Added AdMob Mediation adapter version tracking
  * Automatically detects and reports all installed AdMob mediation adapters
  * Extracts version information from mediation dependency XML files
  * Combines Android and iOS versions in format: "android_X.X.X_ios_Y.Y.Y"
  * Added `GetAdMobMediationVersions()` method to scan mediation adapters
  * Enhanced `GoogleAdMob` SDK info to include detailed mediation adapter versions
  * Supports all standard mediation adapters (AppLovin, IronSource, MetaAudienceNetwork, etc.)

* **IMPROVEMENT**: Enhanced directory search patterns for flexible project structures
  * Updated all SDK detection methods to use flexible directory search patterns
  * Changed from hardcoded paths to pattern-based searches (e.g., `*GoogleMobileAds`, `*Firebase`)
  * Improved compatibility with diverse Unity project folder organizations
  * Fixed issues where SDKs installed in non-standard locations weren't detected
  * Applied consistent search patterns across AdMob, Firebase, Adjust, and other SDK detection methods
  * Optimized performance while maintaining robust detection capabilities

## [1.2.0] - 2025-06-12

* **NEW FEATURE**: Added render pipeline detection to build information
  * Automatically detects and reports current render pipeline (Built-in, URP, HDRP, or Custom)
  * Added `RenderPipeline` property to `ProjectInfo` class

* **NEW FEATURE**: Added UnicoAPIClient NuGet package detection to SDK information
  * Automatically detects UnicoAPIClient main package version from packages.config
  * Added `GetUnicoAPIClientVersion()` method

## [1.1.1] - 2025-02-11

* Documentation and Changelog urls are updated

## [1.1.0] - 2025-02-11

* `UnicoVersionTrackerProgressBar` is implemented
* `UnicoVersionExporter.ExportBuildInfo` and `UnicoVersionExporter.ExportSdkInfo` methods are converted to async
* `GetSavedBuildInfo` and `GetSavedBuildInfoJson` public methods are added to UnicoVersionExporter

## [1.0.0] - 2025-01-24

* This is the first release of Unity Package UnicoVersionTracker.