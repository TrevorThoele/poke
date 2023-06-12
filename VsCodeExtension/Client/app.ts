import * as path from 'path';
import { workspace, ExtensionContext } from 'vscode';

import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    ErrorHandler,
    ErrorHandlerResult,
    ErrorAction,
    CloseAction,
    Message
} from 'vscode-languageclient/node';

let client: LanguageClient;

const errorHandler: ErrorHandler = {
    error: (error: Error, message: Message | undefined, count: number | undefined): ErrorHandlerResult | Promise<ErrorHandlerResult> => {
        return {
            action: ErrorAction.Continue
        }
    },
    closed: () => {
        return {
            action: CloseAction.DoNotRestart
        }
    }
};

export function activate(context: ExtensionContext) {
    const command = context.asAbsolutePath(path.join('../', 'Server', 'bin', 'Debug', 'net6.0', 'Server.exe'));

    const executable = {
        command,
        args: []
    };

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: ServerOptions = {
        run: executable,
        debug: executable
    };

    // Options to control the language client
    const clientOptions: LanguageClientOptions = {
        // Register the server for plain text documents
        documentSelector: [{ scheme: 'file', language: 'csharp' }],
        synchronize: {
            // Notify the server about file changes to '.clientrc files contained in the workspace
            fileEvents: workspace.createFileSystemWatcher('**/.clientrc')
        },
        errorHandler
    };

    // Create the language client and start the client.
    client = new LanguageClient(
        'languageServerExample',
        'Language Server Example',
        serverOptions,
        clientOptions
    );

    // Start the client. This will also launch the server
    client.start();
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
