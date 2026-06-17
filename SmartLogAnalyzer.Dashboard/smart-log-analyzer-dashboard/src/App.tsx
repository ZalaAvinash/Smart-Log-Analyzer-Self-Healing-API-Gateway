import React, { useEffect, useState, useCallback } from 'react';
import './App.css';
import * as signalR from '@microsoft/signalr';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5206';

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
  const [loading, setLoading] = useState(true);
  const [expandedErrors, setExpandedErrors] = useState<Set<number>>(new Set());
  const [analyzingIds, setAnalyzingIds] = useState<Set<number>>(new Set());

  const toggleExpand = useCallback((id: number) => {
    setExpandedErrors(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const deleteError = useCallback(async (id: number) => {
    try {
      await fetch(`${API_URL}/api/error/${id}`, { method: 'DELETE' });
      setErrors(prev => prev.filter(e => e.id !== id));
    } catch (err) {
      console.error('Delete error:', err);
    }
  }, []);

  const clearAllErrors = useCallback(async () => {
    if (!window.confirm('Are you sure you want to clear all errors?')) return;
    try {
      await fetch(`${API_URL}/api/error/clear`, { method: 'DELETE' });
      setErrors([]);
    } catch (err) {
      console.error('Clear errors:', err);
    }
  }, []);

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
              setErrors(prev => {
                const index = prev.findIndex(e => e.id === error.id);
                if (index !== -1) {
                  const updated = [...prev];
                  updated[index] = error;
                  return updated;
                }
                return [error, ...prev];
              });
              // If AI analysis just completed, remove from analyzing set
              if (error.aiRootCause) {
                setAnalyzingIds(prev => {
                  const next = new Set(prev);
                  next.delete(error.id);
                  return next;
                });
              }
            } catch (e) {
              console.error('Error parsing error update:', e);
            }
          });
        })
        .catch((e) => {
          console.log('SignalR Connection Error: ', e);
          setConnected(false);
        });

      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  useEffect(() => {
    fetch(`${API_URL}/api/error`)
      .then((res) => res.json())
      .then((data) => {
        setErrors(data);
        setLoading(false);
      })
      .catch((err) => {
        console.error('Fetch error:', err);
        setLoading(false);
      });
  }, []);

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric',
      hour: '2-digit', minute: '2-digit', second: '2-digit'
    });
  };

  const getSeverity = (error: ErrorLog) => {
    if (error.occurrenceCount >= 10) return 'critical';
    if (error.occurrenceCount >= 5) return 'high';
    if (error.occurrenceCount >= 2) return 'medium';
    return 'low';
  };

  const getSeverityLabel = (severity: string) => {
    switch (severity) {
      case 'critical': return '🔴 Critical';
      case 'high': return '🟠 High';
      case 'medium': return '🟡 Medium';
      default: return '🟢 Low';
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <div className="header-left">
          <h1>🔍 Smart Log Analyzer</h1>
          <p className="subtitle">AI-Powered Error Monitoring & Self-Healing</p>
        </div>
        <div className="header-right">
          <div className="connection-status">
            <span className={`status-dot ${connected ? 'connected' : 'disconnected'}`}></span>
            <span className="status-text">{connected ? 'Live' : 'Disconnected'}</span>
          </div>
          <div className="error-count">
            <span className="count-badge">{errors.length}</span>
            <span className="count-label">Errors</span>
          </div>
        </div>
      </header>

      <main>
        {loading ? (
          <div className="loading-container">
            <div className="spinner"></div>
            <p>Loading errors...</p>
          </div>
        ) : errors.length === 0 ? (
          <div className="no-errors">
            <div className="no-errors-icon">✅</div>
            <h2>All Systems Operational</h2>
            <p>No errors detected. Your application is running smoothly.</p>
            <div className="test-links">
              <p>Test the system:</p>
              <a href={`${API_URL}/api/triggererror`} target="_blank" rel="noopener noreferrer" className="test-link">
                Trigger Test Error →
              </a>
            </div>
          </div>
        ) : (
          <>
            <div className="toolbar">
              <h2>Error Log <span className="error-count-small">({errors.length})</span></h2>
              <button className="clear-btn" onClick={clearAllErrors}>Clear All</button>
            </div>
            <div className="error-list">
              {errors.map((error) => {
                const severity = getSeverity(error);
                const isExpanded = expandedErrors.has(error.id);
                const isAnalyzing = analyzingIds.has(error.id);
                const hasAiAnalysis = !!error.aiRootCause && error.aiRootCause !== 'Failed to parse AI response.';

                return (
                  <div key={error.id} className={`error-card severity-${severity}`}>
                    <div className="error-card-header" onClick={() => toggleExpand(error.id)}>
                      <div className="error-card-left">
                        <span className="severity-badge">{getSeverityLabel(severity)}</span>
                        <h3 className="error-title">{error.errorMessage}</h3>
                      </div>
                      <div className="error-card-right">
                        <span className="occurrence-count" title="Occurrences">
                          🔄 {error.occurrenceCount}
                        </span>
                        <span className="timestamp">{formatDate(error.timestamp)}</span>
                        <span className={`expand-icon ${isExpanded ? 'expanded' : ''}`}>▼</span>
                      </div>
                    </div>

                    {isExpanded && (
                      <div className="error-card-body">
                        <div className="error-meta">
                          <div className="meta-item">
                            <span className="meta-label">Route</span>
                            <code className="meta-value">{error.routePath}</code>
                          </div>
                          <div className="meta-item">
                            <span className="meta-label">Error ID</span>
                            <code className="meta-value">#{error.id}</code>
                          </div>
                        </div>

                        {error.stackTrace && (
                          <details className="stack-trace-details">
                            <summary>📋 Stack Trace</summary>
                            <pre className="stack-trace">{error.stackTrace}</pre>
                          </details>
                        )}

                        <div className="ai-section">
                          <h4>🤖 AI Analysis</h4>
                          {isAnalyzing ? (
                            <div className="ai-loading">
                              <div className="ai-spinner"></div>
                              <span>Analyzing error with AI...</span>
                            </div>
                          ) : hasAiAnalysis ? (
                            <div className="ai-analysis">
                              <div className="ai-item">
                                <span className="ai-label">Root Cause</span>
                                <p className="ai-text">{error.aiRootCause}</p>
                              </div>
                              <div className="ai-item">
                                <span className="ai-label">Fix Suggestion</span>
                                <p className="ai-text">{error.aiFixSuggestion}</p>
                              </div>
                              {error.aiCodePatch && error.aiCodePatch !== 'N/A' && (
                                <div className="ai-item">
                                  <span className="ai-label">Code Patch</span>
                                  <pre className="code-patch">{error.aiCodePatch}</pre>
                                </div>
                              )}
                            </div>
                          ) : (
                            <div className="ai-pending">
                              <span>⏳ Waiting for AI analysis...</span>
                            </div>
                          )}
                        </div>

                        <div className="error-actions">
                          <button className="delete-btn" onClick={() => deleteError(error.id)}>
                            🗑️ Delete
                          </button>
                        </div>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          </>
        )}
      </main>

      <footer className="App-footer">
        <p>Smart Log Analyzer & Self-Healing API Gateway © 2026</p>
      </footer>
    </div>
  );
}

export default App;