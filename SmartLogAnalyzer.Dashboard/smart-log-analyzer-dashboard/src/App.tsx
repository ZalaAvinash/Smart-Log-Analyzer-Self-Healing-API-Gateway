import React, { useEffect, useState } from 'react';
import './App.css';
import * as signalR from '@microsoft/signalr';

const API_URL = 'http://localhost:5206';

interface ErrorLog {
  id: number;
  errorMessage: string;
  stackTrace: string;
  routePath: string;
  timestamp: string;
  occurrenceCount: number;
  aiRootCause?: string;
  aiFixSuggestion?: string;
  aiCodePatch?: string;
}

function App() {
  const [errors, setErrors] = useState<ErrorLog[]>([]);
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/errorHub`)
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log('SignalR Connected');
          setConnected(true);
          connection.on('ReceiveErrorUpdate', (errorJson: string) => {
            try {
              const error: ErrorLog = JSON.parse(errorJson);
              setErrors((prev) => {
                const index = prev.findIndex((e) => e.id === error.id);
                if (index !== -1) {
                  const updated = [...prev];
                  updated[index] = error;
                  return updated;
                }
                return [error, ...prev];
              });
            } catch (e) {
              console.error('Error parsing error update:', e);
            }
          });
        })
        .catch((e) => {
          console.log('SignalR Connection Error: ', e);
          setConnected(false);
        });
    }
  }, [connection]);

  useEffect(() => {
    fetch(`${API_URL}/api/error`)
      .then((res) => res.json())
      .then((data) => setErrors(data))
      .catch((err) => console.error('Fetch error:', err));
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <h1>🔍 Smart Log Analyzer Dashboard</h1>
        <div className="connection-status">
          <span className={`status-dot ${connected ? 'connected' : 'disconnected'}`}></span>
          {connected ? 'Connected' : 'Disconnected'}
        </div>
      </header>
      <main>
        {errors.length === 0 ? (
          <div className="no-errors">
            <h2>✅ No errors detected</h2>
            <p>Trigger an error by visiting <code>{API_URL}/api/triggererror</code></p>
          </div>
        ) : (
          <div className="error-list">
            <h2>Errors ({errors.length})</h2>
            {errors.map((error) => (
              <div key={error.id} className="error-card">
                <div className="error-header">
                  <h3>{error.errorMessage}</h3>
                  <span className="timestamp">{new Date(error.timestamp).toLocaleString()}</span>
                </div>
                <div className="error-body">
                  <p><strong>Route:</strong> {error.routePath}</p>
                  <p><strong>Occurrences:</strong> {error.occurrenceCount}</p>
                  {error.stackTrace && (
                    <details>
                      <summary>Stack Trace</summary>
                      <pre className="stack-trace">{error.stackTrace}</pre>
                    </details>
                  )}
                  {error.aiRootCause && (
                    <div className="ai-analysis">
                      <h4>🤖 AI Analysis</h4>
                      <p><strong>Root Cause:</strong> {error.aiRootCause}</p>
                      <p><strong>Fix Suggestion:</strong> {error.aiFixSuggestion}</p>
                      {error.aiCodePatch && (
                        <div className="code-patch">
                          <strong>Code Patch:</strong>
                          <pre>{error.aiCodePatch}</pre>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}

export default App;