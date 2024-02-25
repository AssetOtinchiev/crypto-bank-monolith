# 3.  Use Modular Monolith System Architecture

Date: 2024-02-25

Log date: 2024-02-25

## Status

Accepted

## Context

An advanced example of Modular Monolith architecture and tactical DDD implementation in .NET is missing on the internet.

## Decision

We decided to create nontrivial application using Modular Monolith architecture and Domain-Driven Design tactical patterns.

## Consequences

- All modules must run in one single process as single application (Monolith)
- All modules should have maximum autonomy (Modular)
- DDD Bounded Contexts will be used to divide monolith into modules
- DDD tactical patterns will be used to implement most of modules