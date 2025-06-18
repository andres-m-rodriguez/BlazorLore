# Internal Publishing Guide

This guide is for maintainers only. Do not include in the NuGet package.

## Publishing via GitHub Actions

### Automated Release Publishing

1. Create a new release on GitHub
2. Tag it with a version (e.g., `v1.0.0`)
3. The `publish.yml` workflow will automatically:
   - Build and test the project
   - Pack the NuGet package
   - Publish to NuGet.org
   - Attach the package to the GitHub release

### Manual Publishing

You can manually trigger a publish via Actions:
1. Go to Actions → Publish to NuGet
2. Click "Run workflow"
3. Enter the version number
4. Click "Run workflow"

### Pre-release Publishing

Pre-releases are automatically published from the `develop` branch:
- Every push to `develop` creates a beta version
- Version format: `x.y.z-beta.{commit-count}`
- You can also manually trigger with different suffixes (alpha, beta, rc)

## Setting up Secrets

Required GitHub secrets:
- `NUGET_API_KEY`: Your NuGet.org API key

To add:
1. Go to Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Name: `NUGET_API_KEY`
4. Value: Your API key from nuget.org

## Local Publishing

For local testing before publishing:

```bash
# Pack the tool
dotnet pack -c Release

# Test local installation
dotnet tool install --global --add-source ./bin/Release BlazorLore.Scaffold.Cli

# Test it works
blazor-scaffold --version

# Uninstall test
dotnet tool uninstall --global BlazorLore.Scaffold.Cli

# Publish to NuGet (requires NUGET_API_KEY env var)
dotnet nuget push bin/Release/BlazorLore.Scaffold.Cli.*.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Version Management

- Use semantic versioning: MAJOR.MINOR.PATCH
- Update version in `BlazorLore.Scaffold.Cli.csproj`
- Tag releases in git: `git tag v1.0.0 && git push origin v1.0.0`

## Troubleshooting

- **Package already exists**: Increment version number
- **Build fails**: Check .NET 9 SDK is installed
- **Tests fail**: Fix tests before publishing
- **API key issues**: Regenerate key on nuget.org