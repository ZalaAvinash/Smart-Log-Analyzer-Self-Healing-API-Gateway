# Smart Log Analyzer & Self-Healing API Gateway

## Project Description (350 words)

The **Smart Log Analyzer & Self-Healing API Gateway** is an advanced, AI-driven solution designed to revolutionize application monitoring and error diagnostics. Built on a modern .NET 10 architecture with a React frontend, this project addresses the critical need for proactive error management in distributed systems.

### Core Problem & Solution
Traditional error logging often involves manual log sifting and delayed responses. This project automates the entire lifecycle of an error—from detection to diagnosis to remediation. By leveraging the power of GPT-4o through Microsoft Semantic Kernel, it transforms raw stack traces into actionable intelligence.

### Architecture & Workflow
The system follows a clean, decoupled architecture:
1.  **API Gateway:** A custom middleware intercepts all unhandled exceptions in the ASP.NET Core pipeline. Instead of crashing, it instantly acknowledges the client and offloads the error details to a background job queue (Hangfire).
2.  **Background Processing:** A dedicated Worker Service processes these jobs asynchronously. It first checks a Redis cache for duplicate errors to optimize costs and performance.
3.  **AI Diagnosis:** For unique errors, the system queries Azure OpenAI. The AI is prompted to act as a Senior .NET Engineer, providing a structured JSON response containing the `RootCause`, `FixSuggestion`, and a `CodePatch`.
4.  **Real-Time Dashboard:** Using SignalR, the results are pushed instantly to a React-based live dashboard. Developers can view errors as they occur and copy-paste the suggested code fixes directly into their IDE.

### Key Features
*   **Self-Healing Suggestions:** Reduces debugging time by providing AI-generated code patches.
*   **Cost Optimization:** Redis-based deduplication prevents redundant AI calls for recurring errors.
*   **Real-Time Monitoring:** SignalR enables live updates without page refreshes.
*   **Scalable Infrastructure:** Separation of concerns between Gateway, Worker, and Dashboard allows for independent scaling.

### Technology Stack
*   **Backend:** ASP.NET Core Web API, Worker Service, Entity Framework Core (SQL Server), SignalR, Hangfire, Redis.
*   **AI Integration:** Microsoft Semantic Kernel, Azure OpenAI (GPT-4o).
*   **Frontend:** React, TypeScript.

This project demonstrates proficiency in cloud-native patterns, AI integration, and full-stack development, making it a standout addition to any professional portfolio.