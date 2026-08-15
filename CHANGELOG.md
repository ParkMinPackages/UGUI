# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [11.0.0] - 2026-08-15

### Breaking Changes
- Replaced the prefab-list Presenter creation `Func` with an initialization `Action<TModel, TView, CompositeDisposable>`.

### Changed
- Allow each item initializer to register zero or more lifecycle resources without returning a combined disposable.
- Dispose resources already registered for an item when its initialization fails.

## [10.0.0] - 2026-08-14

### Breaking Changes
- Replaced the prefab-list Presenter creation `Action` with a `Func<TModel, TView, IDisposable>` so each created item can return its lifecycle resource.

### Changed
- Track each item lifecycle resource together with its model and View.
- Dispose item lifecycle resources when entries are removed, replaced, cleared, or when the parent Presenter is disposed.

## [9.0.0] - 2026-08-14

### Breaking Changes
- Replaced `ItemListView<TModel, TItemView>` and `IItemListView<TModel, TItemView>` with the non-generic `PrefabListView` and `IPrefabListView` APIs.
- Renamed `ObservableCollectionItemListPresenter<TModel, TItemView>` to `ObservableCollectionPrefabListPresenter<TModel, TItemView>`.
- Removed the `IItemView` lifecycle contract and the `Initialize` and batch `SetOrder` APIs.

### Added
- Exposed the source prefab and read-only `GameObject` list access from `IPrefabListView`.
- Added customizable creation and removal actions to the observable-collection presenter.
- Added guards for missing prefab parents and prefab replacement while items exist.

### Changed
- Track model and View pairs in one collection and apply sorting through individual move operations.
- Keep item removal lifecycle behavior external so callers can provide animation, pooling, or destruction behavior.

## [8.0.0] - 2026-08-14

### Breaking Changes
- Replaced `SynchronizedViewBuilder` and `ISyncronizedViewItem<TData>` with the separated item View and observable-collection Presenter APIs.

### Added
- Added `IItemView`, `IItemListView<TModel, TItemView>`, and the reusable `ItemListView<TModel, TItemView>` component.
- Added `ObservableCollectionItemListPresenter<TModel, TItemView>` to synchronize add, remove, move, replace, clear, reverse, and sort operations.
- Added ranged reverse and sort handling while preserving the relationship between models and their item Views.

### Changed
- Preserve non-item children while applying synchronized item sibling order.
- Moved the superseded synchronized View prototypes into the temporary source area.

## [7.0.1] - 2026-08-13

### Fixed
- Track synchronized View instances so clearing a source collection removes every previously created View.
- Remove the replaced View before applying the updated synchronized View order.
- Reapply sibling ordering when synchronized Views are sorted or reversed.

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
