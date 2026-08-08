# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [5.1.0] - 2026-08-08

### Added
- Added `UILMMoveFromShowAnimation` and `UILMMoveFromHideAnimation` for offset-based LitMotion transitions.

### Fixed
- Made `UIActivator` activation and deactivation operations complete as no-ops after the component has been destroyed.

## [5.0.1] - 2026-07-25

### Fixed
- Restored the UIActivator Inspector UXML layout from the ParkMinPackages namespace-migration version.
- Added null-safe handling when the UIActivator Inspector layout or its additional reactive-property foldout cannot be found.
## [5.0.0] - 2026-07-25

### Breaking Changes
- Changed runtime and editor namespaces to the `ParkMinPackages.UGUI` convention.
- Moved `BasicUI` and opinionated UI workflow assets to Workflow.Default.
- Renamed the UI animation component group to `UIActivatorAnimations`.

### Added
- Added `MaxValueView` and `MinMaxValueView` components.

### Fixed
- Updated the UIActivator inspector UXML to resolve its current enum type correctly.
## [4.0.0] - 2026-07-25

### Breaking Changes
- Updated runtime and editor namespaces to match the revised package structure.

## [3.0.0] - 2026-07-25

### Breaking Changes
- Reorganized Runtime scripts into Components, Objects, Interfaces, and Enums folders.
- Updated namespaces to match the new folder structure.

## [2.0.0] - 2026-07-25

### Breaking Changes
- Renamed public namespaces and assembly definitions from Mutant to ParkMinPackages.
- Updated serialized UIActivator type references and the Expansion dependency to the new package identity.
- Projects using the previous namespaces or assembly names must update their references.

## [0.1.0] - 2026-04-07

### This is the first release of *\<UGUI\>*.

*Short description of this release*
