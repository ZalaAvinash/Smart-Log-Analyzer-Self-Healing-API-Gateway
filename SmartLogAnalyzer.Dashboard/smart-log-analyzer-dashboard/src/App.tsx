import React, { useEffect, useState } from 'react';
import './App.css';
import * as signalR from '@microsoft/signalr';

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

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7001/errorHub') // Adjust port as needed
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
          connection.on('ReceiveErrorUpdate', (errorJson: string) => {
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
          });
        })
        .catch((e) => console.log('SignalR Connection Error: ', e));
    }
  }, [connection]);

  // Fetch initial errors
  useEffect(() => {
    fetch('https://localhost:7001/api/error') // Adjust port as needed
      .then((res) => res.json())
      .then((data) => setErrors(data))
      .catch((err) => console.error(err));
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <h1>Smart Log Analyzer Dashboard</h1>
      </header>
      <main>
        <div className="error-list">
          {errors.map((error) => (
            <div key={error.id} className="error-card">
              <div className="error-header">
                <h3>{error.errorMessage}</h3>
                <span className="timestamp">{new Date(error.timestamp).toLocaleString()}</span>
              </div>
              <div className="error-body">
                <p><strong>Route:</strong> {error.routePath}</p>
                <p><strong>Occurrences:</strong> {error.occurrenceCount}</p>
                
                {error.aiRootCause && (
                  <div className="ai-analysis">
                    <h4>AI Analysis</h4>
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
      </main>
    </div>
  );
}

export default App;