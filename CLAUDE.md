# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is the Firely CQL SDK - NCQA's and Firely's official SDK for working with Clinical Quality Language (CQL) on .NET. It contains an ELM execution engine and can compile ELM to .NET assemblies.

## Build Commands

```bash
# Restore dependencies
dotnet restore Cql.sln

# Build the solution
dotnet build Cql.sln

# Run all tests
dotnet test Cql.sln

# Run specific test project
dotnet test Cql/CoreTests/CoreTests.csproj
dotnet test Cql/Cql.Firely.Test/Cql.Firely.Test.csproj

# Run a single test by filter
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

## Submodule

This repository includes `firely-net-sdk` as a git submodule:

```bash
# After cloning, initialize submodule
git submodule update --init --recursive

# Update submodule to latest
cd firely-net-sdk && git pull origin feature/profile-code-generator-5.13 && cd ..
git add firely-net-sdk && git commit -m "Update submodule"
```

## Architecture

### Project Layering

The SDK is organized into these key areas:

**Core Runtime** (`Cql/`):
- `Cql.Primitives` - CQL primitive types (CqlDate, CqlDateTime, CqlInterval, CqlQuantity, etc.)
- `Cql.Abstractions` - Core interfaces (IDataSource, IValueSetDictionary)
- `Cql.Operators` - CQL operator implementations
- `Cql.Comparers` - Type comparison logic
- `Cql.Conversion` - TypeConverter for model bindings
- `Cql.ValueSets` - Value set terminology operations
- `Cql.Runtime` - Runtime execution context and CqlContext

**ELM Processing**:
- `Elm` - ELM (Expression Logical Model) type definitions
- `Cql.Compiler` - Compiles ELM to .NET expressions
- `CodeGeneration.NET` - Generates C# source from ELM

**Model Bindings**:
- `Cql.Firely` - Bindings for Hl7.Fhir.Model POCOs (full Firely SDK)
- `Cql.Hedis` - Bindings for lightweight HEDIS DTOs (alternative to Firely)

**Packaging**:
- `Cql.Packaging` - Library packaging utilities
- `PackagerCLI` - Command-line packager tool

### Model Binding Pattern

Both `Cql.Firely` and `Cql.Hedis` follow the same architecture:
- `*TypeResolver` - Maps ELM/FHIR type specifiers to .NET types
- `*TypeConverter` - Converts between model types and CQL primitives
- `*DataSource` - Implements IDataSource for Retrieve operations
- `*CqlContext` - Factory for creating configured CQL contexts

### Key Abstractions

- **CqlContext** - Main runtime context for executing CQL
- **IDataSource** - Interface for data retrieval (RetrieveByCodes, RetrieveByValueSet)
- **TypeConverter** - Handles conversions between types
- **ICqlComparer** - Type comparison for CQL semantics

## HEDIS Integration

The `Cql.Hedis` project provides an alternative to `Cql.Firely` using lightweight DTOs:

- **DTOs location**: `firely-net-sdk/Ncqa.Hedis.Core.2025/`
- **Namespace**: `Ncqa.Hedis.Core._2025`
- **Pattern**: Sealed records with init setters, System.Text.Json serialization
- **Use case**: Faster serialization, smaller memory footprint for HEDIS measures

Key files in `Cql/Cql.Hedis/`:
- `HedisTypeResolver.cs` - Type mapping
- `HedisTypeConverter.cs` - Type conversions
- `HedisDataSource.cs` - Data retrieval with code filtering
- `HedisCqlContext.cs` - Context factory

## Code Conventions

- Target framework: `net9.0`
- C# language version: 11.0
- Nullable reference types: enabled
- Warnings treated as errors
- Shared props file: `cql-sdk.props`

## Active Development Branches

- Main branch: `develop` (Git Flow)
- HEDIS integration: `feature/profile-specific-model`
- Submodule tracks: `feature/profile-code-generator-5.13`

## Testing

Test projects:
- `CoreTests` - Core CQL operator and runtime tests
- `Cql.Firely.Test` - Integration tests with Firely SDK model

Test frameworks: MSTest, FluentAssertions
