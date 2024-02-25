# 2.  Vertical slice architecture

Date: 2024-02-25

Log date: 2024-02-25

## Status

Accepted

## Context

For every service we should use this aproach, because it is easy to scale, support.

We think of the application code in terms of features rather than the layer it sits in. We treat each feature as a vertical slice. Because every feature within the application is a separate component, it can change independently. 

## Decision

We will implement it on every sevice, it bring us low coupling and high cohesion.

## Consequences

- All modules can be develop separately
- All modules should have maximum autonomy
- DDD tactical patterns will be used to implement most of modules