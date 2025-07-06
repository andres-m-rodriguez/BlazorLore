import * as https from 'https';
import * as vscode from 'vscode';

export interface VersionInfo {
    current: string | null;
    latest: string | null;
    isUpdateAvailable: boolean;
}

export class VersionChecker {
    private static readonly NUGET_API_URL = 'https://api.nuget.org/v3-flatcontainer/blazorlore.format.cli/index.json';
    private static readonly CACHE_KEY = 'blazorFormatter.lastUpdateCheck';
    private static readonly CACHE_DURATION = 24 * 60 * 60 * 1000; // 24 hours

    /**
     * Fetch the latest version from NuGet
     */
    public static async getLatestVersion(): Promise<string | null> {
        return new Promise((resolve) => {
            https.get(this.NUGET_API_URL, (res) => {
                let data = '';
                
                res.on('data', (chunk) => {
                    data += chunk;
                });
                
                res.on('end', () => {
                    try {
                        const json = JSON.parse(data);
                        const versions = json.versions || [];
                        if (versions.length > 0) {
                            // Get the latest stable version (non-prerelease)
                            const stableVersions = versions.filter((v: string) => !v.includes('-'));
                            const latest = stableVersions.length > 0 
                                ? stableVersions[stableVersions.length - 1]
                                : versions[versions.length - 1];
                            resolve(latest);
                        } else {
                            resolve(null);
                        }
                    } catch (error) {
                        console.error('Failed to parse NuGet response:', error);
                        resolve(null);
                    }
                });
            }).on('error', (error) => {
                console.error('Failed to fetch from NuGet:', error);
                resolve(null);
            });
        });
    }

    /**
     * Compare two version strings
     */
    private static compareVersions(v1: string, v2: string): number {
        const parts1 = v1.split('.').map(n => parseInt(n, 10));
        const parts2 = v2.split('.').map(n => parseInt(n, 10));
        
        for (let i = 0; i < Math.max(parts1.length, parts2.length); i++) {
            const part1 = parts1[i] || 0;
            const part2 = parts2[i] || 0;
            
            if (part1 > part2) return 1;
            if (part1 < part2) return -1;
        }
        
        return 0;
    }

    /**
     * Check if an update is available
     */
    public static async checkForUpdate(currentVersion: string | null): Promise<VersionInfo> {
        if (!currentVersion) {
            return {
                current: null,
                latest: null,
                isUpdateAvailable: false
            };
        }

        const latestVersion = await this.getLatestVersion();
        
        if (!latestVersion) {
            return {
                current: currentVersion,
                latest: null,
                isUpdateAvailable: false
            };
        }

        const isUpdateAvailable = this.compareVersions(latestVersion, currentVersion) > 0;
        
        return {
            current: currentVersion,
            latest: latestVersion,
            isUpdateAvailable
        };
    }

    /**
     * Check if we should perform an update check (respects cache)
     */
    public static shouldCheckForUpdate(context: vscode.ExtensionContext): boolean {
        const config = vscode.workspace.getConfiguration('blazorFormatter');
        if (!config.get<boolean>('checkForUpdates', true)) {
            return false;
        }

        const lastCheck = context.globalState.get<number>(this.CACHE_KEY, 0);
        const now = Date.now();
        
        return (now - lastCheck) > this.CACHE_DURATION;
    }

    /**
     * Update the last check timestamp
     */
    public static async updateLastCheckTime(context: vscode.ExtensionContext): Promise<void> {
        await context.globalState.update(this.CACHE_KEY, Date.now());
    }
}