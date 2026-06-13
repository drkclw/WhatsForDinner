# Contracts: Azure IaC and CI/CD Baseline

This directory captures deployment-facing contracts for the infrastructure and workflow interfaces introduced by this feature.

## Files

- deployment-contract.md: Canonical contract for Bicep inputs/outputs and GitHub Actions workflow interface

## Scope

Contracts in this folder are implementation-agnostic and define:
- Required deployment inputs
- Expected deployment outputs
- CI/CD authentication and stage behavior guarantees
- Exclusions and boundaries (for example, no Azure OpenAI creation)
