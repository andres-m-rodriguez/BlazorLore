import * as vscode from 'vscode';
import { BlazorFormatter } from './formatter';

export class BlazorFormattingProvider implements vscode.DocumentFormattingEditProvider {
    constructor(private formatter: BlazorFormatter) {}

    async provideDocumentFormattingEdits(
        document: vscode.TextDocument,
        options: vscode.FormattingOptions,
        token: vscode.CancellationToken
    ): Promise<vscode.TextEdit[]> {
        if (token.isCancellationRequested) {
            return [];
        }

        try {
            const text = document.getText();
            const formatted = await this.formatter.format(text, document.uri.fsPath);

            if (formatted === text) {
                return [];
            }

            // Replace entire document
            const fullRange = new vscode.Range(
                document.positionAt(0),
                document.positionAt(text.length)
            );

            return [vscode.TextEdit.replace(fullRange, formatted)];
        } catch (error) {
            console.error('Formatting failed:', error);
            throw error;
        }
    }
}