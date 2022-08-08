# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes within the 'Internal' namespace do note adhere to semanatic versioning, e.g. the constructs 
within that namespace may have breaking changes without a major version update.


## [Unreleased]
### Added
- Support for existing dependencies to be used as parameters in behavior constructors.
- App.Of so a behavior can retrieve its App to perform contextualization and decontextualization.
- Teardown operations.
- Support for mutualistic relationships between contexts with the 'mutualism' attribute.
- Support for mutual state binding for mutualistic contexts.
- Built-in console logging with the ConsoleOutput context.
- Internal support for update operations.

### Changed
- Contextualization and decontextualization behavior instantiations and destructions now occur 
as part of the update cycle.
- Context states provide their base hashcode instead of their value's hashcode.


## [1.1.1] - 2022-03-07
### Added
- Changelog file.

### Fixed
- Initialization Behaviors can't use App Calls in Constructor


## [1.1.0] - 2022-02-15
### Added
- Support for existing dependencies.
- Fulfilled behavior instantiation.
- Concepts project for detailing various concepts of contextual programming through the SDK.
- Generic read-only context states.
- Generic read-only context state lists.


## [1.0.0] - 2022-01-02
### Added
- The Contextual Programming project.
- Initial implementation from the As Legacy project.
- Test code setup.
- Context attribute and related functionality.
- Behavior dependency attribute and related functionality.
- Dependency self-creation.
- Context retrieval through app.
- Context decontextualization.
- Generic context state.
- Operation declarations to run for context changes.
- Contextual state evaluation.
- Context state lists.

### Changed
- Refactored to support Test-Driven Development.
- Split showcase app from the test project.


[Unreleased]: https://github.com/lstertz/ContextualProgramming/compare/v1.1.1...HEAD
[1.1.1]: https://github.com/lstertz/ContextualProgramming/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/lstertz/ContextualProgramming/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/lstertz/ContextualProgramming/releases/tag/v1.0.0
