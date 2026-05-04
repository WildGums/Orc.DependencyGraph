# Orc.DependencyGraph

Orc.DependencyGraph is a .NET library that provides a directed acyclic graph (DAG) implementation for managing and resolving dependency ordering. It supports topological sorting, level-based node queries, and cycle detection.

---

## Critical Rules (Read First)

These rules are **non-negotiable**. Violating them causes broken builds, crashes, or downstream breakage.

### 1. ABI / API Stability

This project maintains stable ABI / API. Breaking changes break downstream apps.

| Allowed | Never |
|---------|-------|
| Add new overloads | Modify existing signatures |
| Add new methods | Remove public APIs |
| Add new classes | Change return types |

### 2. Tests Are Mandatory

**Building alone is NOT sufficient.** Run tests before claiming completion (see [Commands](#commands)).

### 3. Branch Protection (COMPLIANCE REQUIRED)

**Direct commits to protected branches are a policy violation.**

| Repository | Protected Branches |
|------------|-------------------|
| Orc.DependencyGraph | `master` |
| Orc.DependencyGraph | `develop` |

**Required workflow:**

1. **Create a feature branch FIRST** — Use naming convention: `feature/issue-NNNN-description`
2. **Make all commits on the feature branch** — Never commit directly to protected branches
3. **Submit a Pull Request** — Changes must be reviewed by a human before merging

```bash
# CORRECT — Always create a feature branch first
git checkout -b feature/issue-1234-fix-description

# NEVER DO THIS — Policy violation
git checkout develop && git commit  # FORBIDDEN

# NEVER DO THIS — Policy violation
git checkout master && git commit  # FORBIDDEN
```

---

## Commands

Single source of truth for all commands:

| Task | Command |
|------|---------|
| **Build** | `dotnet cake --target=build` |
| **Test** | `dotnet cake --target=test` |
| **Build and test** | `dotnet cake --target=buildandtest` |

---

## Architecture & Directories

### Solution Overview

```
Orc.DependencyGraph         => Core library: graph interfaces and implementations
Orc.DependencyGraph.Tests   => NUnit test suite
Orc.DependencyGraph.Example => Usage example application
```

### Key Namespaces

| Namespace | Purpose |
|-----------|---------|
| `Orc.DependencyGraph` | Public API: `IGraph<T>`, `INode<T>`, graph implementations |
| `Orc.DependencyGraph.GraphB` | `GraphB<T>` — alternative graph implementation |
| `Orc.DependencyGraph.GraphD` | `Graph<T>`, `GraphFast<T>` — primary graph implementations |

### Directory Guide

| Directory | Editable? | Notes |
|-----------|-----------|-------|
| `src/Orc.DependencyGraph/` | Yes | Core library source |
| `src/Orc.DependencyGraph.Tests/` | Yes | NUnit tests |
| `src/Orc.DependencyGraph.Example/` | Yes | Example application |
| `deployment/` | No | Deployment / build scripts |

### Core Interfaces

- **`IGraph<T>`** — The main graph interface. Supports adding sequences, topological sorting, node queries by level, cycle detection.
- **`INode<T>`** — Represents a single node in the graph. Exposes `Value`, `Level`, predecessors, and successors.

---

## Writing Code

### Anti-Patterns (Never Do This)

| Anti-Pattern | Why |
|-------------|-----|
| Modifying method signatures of `IGraph<T>` or `INode<T>` | ABI breaking |
| Using default parameters in public APIs | ABI breaking |
| **Skipping failing tests** | **Unacceptable — tests must pass** |

### Graph Implementations

There are multiple `IGraph<T>` implementations. When adding a feature or fixing a bug in the graph logic, ensure it is addressed consistently across all implementations:

- `Graph<T>` (in `GraphD/`)
- `GraphFast<T>` (in `GraphD/`)
- `GraphB<T>` (in `GraphB/`)

---

## Testing & Debugging

### Running Tests

```bash
dotnet cake --target=test
```

### Tests MUST Pass

> **NON-NEGOTIABLE:** Tests must PASS before claiming completion.
>
> - Do NOT skip failing tests
> - Do NOT claim completion if tests fail

### Writing Tests

1. Use NUnit to write tests
2. Tests are parameterized over all graph implementations using `[TestFixture(typeof(Graph<>))]`
3. Use Pascal_Snake_Case (PascalCase words separated by underscores) for test method names (e.g. `Feature_Does_Work`)

```csharp
[TestFixture(typeof(Graph<>))]
[TestFixture(typeof(GraphFast<>))]
[TestFixture(typeof(GraphB<>))]
public class MyFeatureTest
{
    public MyFeatureTest(Type targetGenericGraph)
    {
        TargetGraph = targetGenericGraph.MakeGenericType(typeof(int));
    }

    private Type TargetGraph { get; set; }

    [Test]
    public void Feature_Does_Work()
    {
        var graph = (IGraph<int>)Activator.CreateInstance(TargetGraph)!;
        graph.AddSequence(new[] { 1, 2, 3 });

        Assert.That(graph.CountNodes, Is.EqualTo(3));
    }
}
```

**Philosophy:** Tests FAIL when wrong, never skip.

### Debugging Methodology

1. **Establish baseline** — What's the known-good state?
2. **One change at a time** — Verify each change before proceeding
3. **Track changes in a table** — Log what you changed and the result
4. **Check all implementations** — Bugs and fixes often apply to `Graph<T>`, `GraphFast<T>`, and `GraphB<T>` equally
5. **Revert if worse** — Don't pile fixes on top of failures

---

## Further Reading

| Topic | Document |
|-------|---------|
| Contributing guidelines | [CONTRIBUTING.md](.github/CONTRIBUTING.md) |
| Project documentation | [WildGums Open Source Portal](http://opensource.wildgums.com) |
