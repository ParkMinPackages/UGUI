# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [7.0.0] - 2026-08-12

### Breaking Changes
- Replaced the `UIAnimation` start/end application contract with per-transition value capture and restoration.
- Removed `Canceled` from `UIActivationState` and `Stop` from `AnimationCancelBehaviour` so every completed or canceled transition resolves to an active or inactive state.
- Renamed the LitMotion move components to `UIMoveFromActiveAnimation` and `UIMoveFromDeactivateAnimation`.
- Renamed fade and scale effect fields to `Alpha` and `Scale`; existing serialized values are preserved through migration attributes.

### Changed
- Capture current position, scale, and alpha values before every transition instead of retaining a permanent snapshot.
- Restore captured presentation values after activation, deactivation, cancellation, and unexpected animation failures while using the Canvas state for stable visibility.
- Allow activation and deactivation without animation components to complete immediately through the same transition path.

### Fixed
- Preserve the previous stable Canvas and presentation state when an unexpected animation exception is propagated.

## [6.0.0] - 2026-08-11

### Breaking Changes
- Replaced the reactive `UIActivator` transition model with explicit `Active`, `Inactive`, and `Canceled` states.
- Renamed deactivation APIs from `DeActive` to `Deactivate` and removed forced transition execution.
- Replaced captured animation values with explicit `ApplyStart` and `ApplyEnd` animation contracts.
- Renamed show and hide animations to active and deactivate animations.
- Moved LitMotion and DOTween animations into dedicated `LitMotions` and `DOTweens` namespaces and renamed their concrete components.

### Added
- Added `AnimationCancelBehaviour` with `Stop`, `Complete`, and `ResetToStart` policies as transition parameters.
- Added serialized start and end values to the UI animation components.

### Changed
- Simplified transition error handling so caller cancellation is rethrown and unexpected errors propagate unchanged.
- Preserved animation component GUIDs while reorganizing their folders and namespaces.
- Updated the `UIActivator` Inspector to use the renamed transition APIs and animation components.

### Removed
- Removed hierarchy-wide reactive state tracking and internal linked cancellation token management from `UIActivator`.

## [5.2.1] - 2026-08-10

### Changed
- Normalized the runtime assembly definition serialization format.

## [5.2.0] - 2026-08-09

### Added
- Added an optional runtime start local position override to `UIActivator`.
- Added conditional start-position controls to the `UIActivator` Inspector.

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
