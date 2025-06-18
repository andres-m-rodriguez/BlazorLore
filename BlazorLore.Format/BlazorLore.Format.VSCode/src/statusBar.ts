import * as vscode from 'vscode';

export class StatusBarManager implements vscode.Disposable {
    private statusBarItem: vscode.StatusBarItem;
    private hideTimeout?: NodeJS.Timeout;

    constructor() {
        this.statusBarItem = vscode.window.createStatusBarItem(
            vscode.StatusBarAlignment.Right,
            100
        );
        this.statusBarItem.command = 'blazorFormatter.formatDocument';
    }

    setFormatting() {
        this.clearTimeout();
        this.statusBarItem.text = '$(sync~spin) Formatting...';
        this.statusBarItem.tooltip = 'Formatting Blazor/Razor file';
        this.statusBarItem.show();
    }

    setSuccess() {
        this.clearTimeout();
        this.statusBarItem.text = '$(check) Formatted';
        this.statusBarItem.tooltip = 'File formatted successfully';
        this.statusBarItem.show();
        this.hideAfterDelay(2000);
    }

    setAlreadyFormatted() {
        this.clearTimeout();
        this.statusBarItem.text = '$(check) Already formatted';
        this.statusBarItem.tooltip = 'File is already properly formatted';
        this.statusBarItem.show();
        this.hideAfterDelay(2000);
    }

    setError() {
        this.clearTimeout();
        this.statusBarItem.text = '$(error) Format failed';
        this.statusBarItem.tooltip = 'Failed to format file';
        this.statusBarItem.backgroundColor = new vscode.ThemeColor('statusBarItem.errorBackground');
        this.statusBarItem.show();
        this.hideAfterDelay(3000);
    }

    private hideAfterDelay(ms: number) {
        this.hideTimeout = setTimeout(() => {
            this.statusBarItem.hide();
            this.statusBarItem.backgroundColor = undefined;
        }, ms);
    }

    private clearTimeout() {
        if (this.hideTimeout) {
            clearTimeout(this.hideTimeout);
            this.hideTimeout = undefined;
        }
        this.statusBarItem.backgroundColor = undefined;
    }

    dispose() {
        this.clearTimeout();
        this.statusBarItem.dispose();
    }
}