# RolePlayingPrototype: technical debt

## Unit collision filtering

`UnitCollisionService` currently calls `Physics.IgnoreCollision` for every pair of registered unit colliders. This avoids changing shared Unity physics settings and is acceptable for the current prototype, but registration cost grows quadratically with the number of units.

Revisit after profiling a mobile build with 100 and 200 simultaneously active units. If registration or physics setup becomes significant, move units to a dedicated physics layer and configure the collision matrix after confirming that changing shared project settings is safe for the other branches.

## Spatial queries

`EnemyDetectionSystem` and `UnitSeparationSystem` currently scan active ECS entities. This is simple and sufficient until profiling proves otherwise, but the work grows quadratically as the unit count increases.

Profile the fixed-update cost with 100 and 200 active units on the target mobile device. If these systems become a bottleneck, introduce a shared spatial hash or uniform grid updated by ECS systems and query only nearby cells for detection and separation.
