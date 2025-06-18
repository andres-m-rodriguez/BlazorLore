import * as vscode from 'vscode';
import { BlazorFormattingProvider } from './formattingProvider';
import { BlazorFormatter } from './formatter';
import { StatusBarManager } from './statusBar';
import { CliInstaller } from './cliInstaller';

let statusBar: StatusBarManager;

export async function activate(context: vscode.ExtensionContext) {
    console.log('Blazor Formatter extension is now active');

    // Check if CLI is installed on activation
    const isInstalled = await CliInstaller.ensureInstalled();
    if (!isInstalled) {
        vscode.window.showWarningMessage(
            'BlazorLore formatter CLI is not installed. Some features may not work correctly.',
            'Install Now'
        ).then(async (choice) => {
            if (choice === 'Install Now') {
                await CliInstaller.promptInstall();
            }
        });
    }

    const formatter = new BlazorFormatter();
    statusBar = new StatusBarManager();

    // Register document formatting provider
    const formattingProvider = new BlazorFormattingProvider(formatter);
    
    const disposable = vscode.languages.registerDocumentFormattingEditProvider(
        [
            { scheme: 'file', language: 'razor' },
            { scheme: 'file', language: 'aspnetcorerazor' }
        ],
        formattingProvider
    );

    // Register format command
    const formatCommand = vscode.commands.registerCommand('blazorFormatter.formatDocument', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            return;
        }

        const document = editor.document;
        if (!isBlazorDocument(document)) {
            vscode.window.showWarningMessage('This file is not a Blazor/Razor file');
            return;
        }

        try {
            statusBar.setFormatting();
            const edits = await formattingProvider.provideDocumentFormattingEdits(
                document,
                { tabSize: 4, insertSpaces: true },
                new vscode.CancellationTokenSource().token
            );

            if (edits && edits.length > 0) {
                const workspaceEdit = new vscode.WorkspaceEdit();
                workspaceEdit.set(document.uri, edits);
                await vscode.workspace.applyEdit(workspaceEdit);
                statusBar.setSuccess();
            } else {
                statusBar.setAlreadyFormatted();
            }
        } catch (error) {
            statusBar.setError();
            vscode.window.showErrorMessage(`Failed to format: ${error}`);
        }
    });

    // Register create config command
    const createConfigCommand = vscode.commands.registerCommand('blazorFormatter.createConfig', async () => {
        try {
            await formatter.createConfig();
            vscode.window.showInformationMessage('Created .blazorfmt.json configuration file');
        } catch (error) {
            vscode.window.showErrorMessage(`Failed to create config: ${error}`);
        }
    });

    // Register install/update CLI command
    const installCliCommand = vscode.commands.registerCommand('blazorFormatter.installCli', async () => {
        try {
            await vscode.window.withProgress({
                location: vscode.ProgressLocation.Notification,
                title: 'BlazorLore Formatter',
                cancellable: false
            }, async (progress) => {
                await CliInstaller.install(progress);
            });
        } catch (error: any) {
            vscode.window.showErrorMessage(error.message);
        }
    });

    // Format on save
    const onSaveDisposable = vscode.workspace.onWillSaveTextDocument(async (event) => {
        const config = vscode.workspace.getConfiguration('blazorFormatter');
        if (!config.get<boolean>('formatOnSave', true)) {
            return;
        }

        const document = event.document;
        if (!isBlazorDocument(document)) {
            return;
        }

        event.waitUntil(
            (async () => {
                try {
                    statusBar.setFormatting();
                    const edits = await formattingProvider.provideDocumentFormattingEdits(
                        document,
                        { tabSize: 4, insertSpaces: true },
                        new vscode.CancellationTokenSource().token
                    );

                    if (edits && edits.length > 0) {
                        statusBar.setSuccess();
                        return edits;
                    } else {
                        statusBar.setAlreadyFormatted();
                        return [];
                    }
                } catch (error) {
                    statusBar.setError();
                    console.error('Format on save failed:', error);
                    return [];
                }
            })()
        );
    });

    context.subscriptions.push(
        disposable,
        formatCommand,
        createConfigCommand,
        installCliCommand,
        onSaveDisposable,
        statusBar
    );
}

export function deactivate() {
    if (statusBar) {
        statusBar.dispose();
    }
}

function isBlazorDocument(document: vscode.TextDocument): boolean {
    const lang = document.languageId;
    return lang === 'razor' || lang === 'aspnetcorerazor' || 
           document.fileName.endsWith('.razor') || document.fileName.endsWith('.cshtml');
}