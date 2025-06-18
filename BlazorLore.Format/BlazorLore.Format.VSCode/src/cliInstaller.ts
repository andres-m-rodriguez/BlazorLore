import * as vscode from 'vscode';
import * as cp from 'child_process';
import * as util from 'util';
import * as fs from 'fs';
import * as path from 'path';

const execAsync = util.promisify(cp.exec);

export class CliInstaller {
    private static readonly TOOL_NAME = 'BlazorLore.Format.Cli';
    private static readonly COMMAND_NAME = 'blazorfmt';
    
    /**
     * Check if the CLI tool is installed
     */
    public static async isInstalled(): Promise<boolean> {
        try {
            const { stdout } = await execAsync('dotnet tool list -g');
            return stdout.includes(this.TOOL_NAME);
        } catch (error) {
            return false;
        }
    }
    
    /**
     * Get installed version of the CLI tool
     */
    public static async getInstalledVersion(): Promise<string | null> {
        try {
            const { stdout } = await execAsync('dotnet tool list -g');
            const lines = stdout.split('\n');
            for (const line of lines) {
                if (line.includes(this.TOOL_NAME)) {
                    const parts = line.trim().split(/\s+/);
                    if (parts.length >= 2) {
                        return parts[1]; // Version is the second column
                    }
                }
            }
            return null;
        } catch (error) {
            return null;
        }
    }
    
    /**
     * Install or update the CLI tool
     */
    public static async install(progress?: vscode.Progress<{ message?: string; increment?: number }>): Promise<void> {
        try {
            // Check if .NET SDK is installed
            try {
                await execAsync('dotnet --version');
            } catch {
                throw new Error('.NET SDK is not installed. Please install .NET SDK from https://dotnet.microsoft.com/download');
            }
            
            progress?.report({ message: 'Checking for existing installation...' });
            
            const isInstalled = await this.isInstalled();
            const command = isInstalled 
                ? `dotnet tool update -g ${this.TOOL_NAME}`
                : `dotnet tool install -g ${this.TOOL_NAME}`;
            
            progress?.report({ message: isInstalled ? 'Updating BlazorLore formatter...' : 'Installing BlazorLore formatter...', increment: 50 });
            
            const { stdout, stderr } = await execAsync(command);
            
            if (stderr && !stderr.includes('was successfully')) {
                throw new Error(stderr);
            }
            
            progress?.report({ message: 'Installation complete!', increment: 50 });
            
            // Verify installation
            const version = await this.getInstalledVersion();
            if (!version) {
                throw new Error('Installation succeeded but tool not found. You may need to restart VS Code.');
            }
            
            vscode.window.showInformationMessage(
                `BlazorLore formatter ${isInstalled ? 'updated' : 'installed'} successfully (v${version})`
            );
        } catch (error: any) {
            throw new Error(`Failed to install BlazorLore formatter: ${error.message}`);
        }
    }
    
    /**
     * Prompt user to install the CLI tool
     */
    public static async promptInstall(): Promise<boolean> {
        const choice = await vscode.window.showInformationMessage(
            'BlazorLore formatter CLI is not installed. Would you like to install it now?',
            'Install',
            'Later'
        );
        
        if (choice === 'Install') {
            try {
                await vscode.window.withProgress({
                    location: vscode.ProgressLocation.Notification,
                    title: 'BlazorLore Formatter',
                    cancellable: false
                }, async (progress) => {
                    await this.install(progress);
                });
                return true;
            } catch (error: any) {
                vscode.window.showErrorMessage(error.message);
                return false;
            }
        }
        
        return false;
    }
    
    /**
     * Ensure CLI is installed, prompting if necessary
     */
    public static async ensureInstalled(): Promise<boolean> {
        if (await this.isInstalled()) {
            return true;
        }
        
        return await this.promptInstall();
    }
}