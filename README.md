# ArterialAnalysis

Godot project that identifies arterial blockages and hemorrhages.

## Heavily Modified Version of 3D Dijkstra's Algorithm

The algorithm began with the inspiration of Dijkstra's algorithm.

<img src="https://github.com/matjmase/ArterialAnalysis/blob/main/Screenshots/RawDijkstra.png" width="600" />

## Composition

The algorithm began to mature as I implemented island detection to identify when a aquisition layer of the algorithm splits or joins.

<img src="https://github.com/matjmase/ArterialAnalysis/blob/main/Screenshots/EndpointsSplitsHemorrhages.png" width="600" />

## Lifecycle Eventing

The algorithm used lifecycle events such that I could stack much higher level logic on top. This resulted in the ability to bridge gaps, identify hemorrhages, and place flag appropriately.

<img src="https://github.com/matjmase/ArterialAnalysis/blob/main/Screenshots/FinalDijkstra.png" width="600" />
