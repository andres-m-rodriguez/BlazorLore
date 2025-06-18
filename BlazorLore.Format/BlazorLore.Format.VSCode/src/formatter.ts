import * as vscode from 'vscode';
import * as cp from 'child_process';
import * as path from 'path';
import * as fs from 'fs';

export class BlazorFormatter {
    private getExecutablePath(): string {
        const config = vscode.workspace.getConfiguration('blazorFormatter');
        const configuredPath = config.get<string>('executablePath');
        
        if (configuredPath) {
            return configuredPath;
        }
        
        // Try to find blazorfmt in common locations
        if (process.platform === 'win32') {
            // Windows: Check for .exe in publish folders
            const debugPublishPath = path.join(__dirname, '..', '..', '..', 'BlazorLore.Format.Cli', 'bin', 'Debug', 'net9.0', 'win-x64', 'publish', 'blazorfmt.exe');
            if (fs.existsSync(debugPublishPath)) {
                return debugPublishPath;
            }
            
            const releasePublishPath = path.join(__dirname, '..', '..', '..', 'BlazorLore.Format.Cli', 'bin', 'Release', 'net9.0', 'win-x64', 'publish', 'blazorfmt.exe');
            if (fs.existsSync(releasePublishPath)) {
                return releasePublishPath;
            }
            
            // Legacy locations for backward compatibility
            const localPath = path.join(__dirname, '..', '..', '..', 'BlazorLore.Format.Cli', 'bin', 'Debug', 'net9.0', 'win-x64', 'blazorfmt.exe');
            if (fs.existsSync(localPath)) {
                return localPath;
            }
        } else {
            // Linux/Mac: Check in publish folders
            const debugPublishPath = path.join(__dirname, '..', '..', '..', 'BlazorLore.Format.Cli', 'bin', 'Debug', 'net9.0', 'linux-x64', 'publish', 'blazorfmt');
            if (fs.existsSync(debugPublishPath)) {
                return debugPublishPath;
            }
            
            const releasePublishPath = path.join(__dirname, '..', '..', '..', 'BlazorLore.Format.Cli', 'bin', 'Release', 'net9.0', 'linux-x64', 'publish', 'blazorfmt');
            if (fs.existsSync(releasePublishPath)) {
                return releasePublishPath;
            }
            
            // Legacy locations for backward compatibility
            const localPath = path.join(__dirname, '..', '..', '..', 'BlazorLore.Format.Cli', 'bin', 'Debug', 'net9.0', 'linux-x64', 'blazorfmt');
            if (fs.existsSync(localPath)) {
                return localPath;
            }
        }
        
        // Default to global command
        return 'blazorfmt';
    }

    private buildArgs(useStdin: boolean = true): string[] {
        const config = vscode.workspace.getConfiguration('blazorFormatter');
        const args: string[] = ['format'];
        
        // Always use stdin for formatting to avoid file system issues
        if (useStdin) {
            args.push('-'); // Read from stdin
        }

        // Add configuration overrides
        const configPath = config.get<string>('configPath');
        if (configPath) {
            args.push('--config', configPath);
        }

        const indentSize = config.get<number>('indentSize');
        if (indentSize !== null && indentSize !== undefined) {
            args.push('--indent-size', indentSize.toString());
        }

        const useTabs = config.get<boolean>('useTabs');
        if (useTabs !== null && useTabs !== undefined) {
            args.push('--use-tabs');
        }

        const attributeBreakThreshold = config.get<number>('attributeBreakThreshold');
        if (attributeBreakThreshold !== null && attributeBreakThreshold !== undefined) {
            args.push('--attribute-break-threshold', attributeBreakThreshold.toString());
        }

        const contentBreakThreshold = config.get<number>('contentBreakThreshold');
        if (contentBreakThreshold !== null && contentBreakThreshold !== undefined) {
            args.push('--content-break-threshold', contentBreakThreshold.toString());
        }

        const breakContentWithManyAttributes = config.get<boolean>('breakContentWithManyAttributes');
        if (breakContentWithManyAttributes !== null && breakContentWithManyAttributes !== undefined) {
            args.push('--break-content-with-many-attributes', breakContentWithManyAttributes.toString());
        }

        return args;
    }

    async format(content: string, filePath?: string): Promise<string> {
        return new Promise((resolve, reject) => {
            const executable = this.getExecutablePath();
            const args = this.buildArgs(true); // Always use stdin
            
            const options: cp.ExecFileOptions = {
                cwd: filePath ? path.dirname(filePath) : vscode.workspace.rootPath || undefined,
                windowsVerbatimArguments: false
            };

            // Use execFile for better path handling
            const proc = cp.execFile(executable, args, options, (error, stdout, stderr) => {
                if (error) {
                    if (error.code === 'ENOENT') {
                        // Try to help user install the formatter
                        vscode.window.showErrorMessage(
                            `Formatter not found. Would you like to install it?`,
                            'Install'
                        ).then(async (choice) => {
                            if (choice === 'Install') {
                                const { CliInstaller } = await import('./cliInstaller');
                                await CliInstaller.promptInstall();
                            }
                        });
                        reject(new Error(`Formatter not found at: ${executable}. Make sure 'blazorfmt' is installed.`));
                    } else {
                        reject(new Error(`Formatter failed: ${stderr || error.message}`));
                    }
                } else {
                    resolve(stdout);
                }
            });

            // Write content to stdin
            if (proc.stdin) {
                proc.stdin.write(content);
                proc.stdin.end();
            }
        });
    }

    async createConfig(): Promise<void> {
        const workspaceFolder = vscode.workspace.workspaceFolders?.[0];
        if (!workspaceFolder) {
            throw new Error('No workspace folder open');
        }

        const configPath = path.join(workspaceFolder.uri.fsPath, '.blazorfmt.json');
        
        if (fs.existsSync(configPath)) {
            const overwrite = await vscode.window.showWarningMessage(
                'Configuration file already exists. Overwrite?',
                'Yes',
                'No'
            );
            
            if (overwrite !== 'Yes') {
                return;
            }
        }

        const executable = this.getExecutablePath();
        const args = ['init', '--force'];
        
        return new Promise((resolve, reject) => {
            cp.exec(`${executable} ${args.join(' ')}`, {
                cwd: workspaceFolder.uri.fsPath
            }, (error, stdout, stderr) => {
                if (error) {
                    reject(new Error(`Failed to create config: ${stderr || error.message}`));
                } else {
                    resolve();
                }
            });
        });
    }
}