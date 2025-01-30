# RandomSkunk.EasyLogger

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog],
and this project adheres to [Semantic Versioning].

## [Unreleased]

### Changed

- The `EasyLogger.IncludeScopes` property is no longer init-only. However, once the logger is used, attempting to change `IncludeScopes` does noting.

## [1.0.0-rc1] - 2025-01-30

### Added

- Add initial project, solution, and package structures.
- Add abstract `EasyLogger` and `EasyLogger<T>` classes.
- Add `LogEntry` and `LogAttributes` structs.

[Keep a Changelog]: https://keepachangelog.com/en/1.1.0/
[Semantic Versioning]: https://semver.org/spec/v2.0.0.html

[Unreleased]: https://github.com/bfriesen/RandomSkunk.EasyLogger/compare/v1.0.0-rc1...HEAD
[1.0.0-rc1]: https://github.com/bfriesen/RandomSkunk.Results/compare/v0.0.0...v1.0.0-rc1
