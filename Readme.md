# Clean Architecture Specification

## Overview & Dependency Flow

Dependencies strictly point inward. Outer layers depend on inner layers, while the core **Domain** layer remains fully isolated from external frameworks and technical implementations.

```text
Presentation (Web API)
  │
  ├──► Application
  │      │
  │      ▼
  │    Domain ◄──┐
  │              │
  └──► Infrastructure (implements Application/Domain interfaces)