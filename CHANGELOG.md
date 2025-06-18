# Changelog

All notable changes to this package will be documented in this file.

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