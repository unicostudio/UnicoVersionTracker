# Changelog

All notable changes to this package will be documented in this file.

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