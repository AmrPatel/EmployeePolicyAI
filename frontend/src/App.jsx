import { useState } from "react";
import "./App.css";

const API_URL = "https://localhost:7079";

function App() {
    const [question, setQuestion] = useState("");
    const [messages, setMessages] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [selectedFile, setSelectedFile] = useState(null);
    const [uploadStatus, setUploadStatus] = useState("");

    // Upload and index a policy PDF.
    async function uploadDocument(event) {
        event.preventDefault();

        if (!selectedFile) {
            setUploadStatus("Please select a PDF file.");
            return;
        }

        if (
            selectedFile.type !== "application/pdf" &&
            !selectedFile.name.toLowerCase().endsWith(".pdf")
        ) {
            setUploadStatus("Please select a PDF file.");
            return;
        }

        const formData = new FormData();
        formData.append("file", selectedFile);

        setUploadStatus("Uploading and indexing PDF...");

        try {
            const response = await fetch(
                `${ API_URL }/api/PolicyDocuments/upload-and-index`,
                {
                    method: "POST",
                    body: formData,
                }
            );

            const responseText = await response.text();

            let data;

            try {
                data = JSON.parse(responseText);
            } catch {
                data = { message: responseText };
            }

            if (!response.ok) {
                throw new Error(
                    data.message ||
                    `Upload failed: HTTP ${ response.status }`
                );
            }

            setUploadStatus(
                `Success! ${ data.documentName } was indexed into ${ data.chunkCount } chunks.`
            );

            setSelectedFile(null);
            event.target.reset();
        } catch (err) {
            console.error("Document upload failed:", err);

            setUploadStatus(
                `Upload failed: ${
    err.message || "Could not connect to the API."
}`
            );
        }
    }

    // Ask a question and include the previous conversation.
    async function askQuestion(event) {
        event.preventDefault();

        const userQuestion = question.trim();

        if (!userQuestion || loading) {
            return;
        }

        // Save the current question in the chat.
        setMessages((previous) => [
            ...previous,
            {
                role: "user",
                content: userQuestion,
            },
        ]);

        setQuestion("");
        setError("");
        setLoading(true);

        try {
            // Prepare the conversation that happened BEFORE
            // the current question.
            const conversationHistory = messages
                .filter(
                    (message) =>
                        message.role === "user" ||
                        message.role === "assistant"
                )
                .map((message) => ({
                    role: message.role,
                    content: message.content,
                }));

            const response = await fetch(
                `${ API_URL }/api/Rag/ask`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        question: userQuestion,
                        topK: 3,
                        conversationHistory: conversationHistory,
                    }),
                }
            );

            const data = await response.json();

            if (!response.ok) {
                throw new Error(
                    data.message ||
                    `Request failed: ${ response.status }`
                );
            }

            // Add the AI answer and its source documents.
            setMessages((previous) => [
                ...previous,
                {
                    role: "assistant",
                    content: data.answer || "No answer was returned.",
                    sources: data.sources || [],
                },
            ]);
        } catch (err) {
            console.error("RAG request failed:", err);

            setError(
                "Could not connect to the AI service. Check that your backend and Ollama are running."
            );
        } finally {
            setLoading(false);
        }
    }

    // Clear the current conversation.
    function clearConversation() {
        if (loading) {
            return;
        }

        setMessages([]);
        setQuestion("");
        setError("");
    }

    return (
        <div className="app">
            <header className="header">
                <div className="logo">PolicyAI</div>

                <div className="header-subtitle">
                    Your Employee Policy Assistant
                </div>
            </header>

            <main className="chat-container">
                {/* PDF upload section */}
                <section className="upload-panel">
                    <h2>Upload Employee Policy</h2>

                    <p>
                        Upload a PDF to make its contents searchable
                        by the AI assistant.
                    </p>

                    <form
                        onSubmit={uploadDocument}
                        className="upload-form"
                    >
                        <input
                            type="file"
                            accept=".pdf,application/pdf"
                            onChange={(event) =>
                                setSelectedFile(
                                    event.target.files?.[0] || null
                                )
                            }
                        />

                        <button
                            type="submit"
                            disabled={!selectedFile}
                        >
                            Upload & Index
                        </button>
                    </form>

                    {uploadStatus && (
                        <p
                            className="upload-status"
                            role="status"
                        >
                            {uploadStatus}
                        </p>
                    )}
                </section>

                {/* Welcome section */}
                {messages.length === 0 && (
                    <section className="welcome">
                        <div className="welcome-icon">✦</div>

                        <h1>How can I help you today?</h1>

                        <p>
                            Ask questions about your company's
                            employee policies. I'll use the available
                            policy documents to help you.
                        </p>

                        <div className="suggestions">
                            <button
                                type="button"
                                onClick={() =>
                                    setQuestion("Can I work from home?")
                                }
                            >
                                Can I work from home?
                            </button>

                            <button
                                type="button"
                                onClick={() =>
                                    setQuestion("Can I take paid leave?")
                                }
                            >
                                Can I take paid leave?
                            </button>

                            <button
                                type="button"
                                onClick={() =>
                                    setQuestion(
                                        "What is the attendance policy?"
                                    )
                                }
                            >
                                What is the attendance policy?
                            </button>
                        </div>
                    </section>
                )}

                {/* Conversation messages */}
                <section className="messages" aria-live="polite">
                    {messages.map((message, index) => (
                        <article
                            className={`message ${ message.role }`}
                            key={`${ index } -${ message.role }`}
                        >
                            <div className="message-label">
                                {message.role === "user"
                                    ? "You"
                                    : "PolicyAI"}
                            </div>

                            <div className="message-content">
                                {message.content}
                            </div>
                            {message.sources?.length > 0 && (
                                <div className="sources">
                                    <h3>📚 Policy Sources</h3>

                                    {message.sources.map((source, sourceIndex) => (
                                        <div
                                            className="source-card"
                                            key={`${source.documentName}-${source.chunkNumber}-${sourceIndex}`}
                                        >
                                            <strong>📄 {source.documentName}</strong>

                                            <p>
                                                <strong>Page:</strong>{" "}
                                                {source.pageNumber > 0
                                                    ? source.pageNumber
                                                    : "Page number unavailable"}
                                            </p>

                                            <p>
                                                <strong>Chunk:</strong> {source.chunkNumber}
                                            </p>

                                            <details>
                                                <summary>View supporting text</summary>
                                                <p>{source.content}</p>
                                            </details>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </article>
                    ))}

                    {loading && (
                        <div className="loading">
                            PolicyAI is thinking...
                        </div>
                    )}
                </section>

                {/* Error message */}
                {error && (
                    <div className="error-message" role="alert">
                        {error}
                    </div>
                )}

                {/* Chat input */}
                <form
                    className="chat-form"
                    onSubmit={askQuestion}
                >
                    <input
                        type="text"
                        value={question}
                        onChange={(event) =>
                            setQuestion(event.target.value)
                        }
                        placeholder="Ask a question about employee policies..."
                        aria-label="Your policy question"
                        disabled={loading}
                    />

                    <button
                        type="submit"
                        disabled={loading || !question.trim()}
                    >
                        {loading ? "Thinking..." : "Ask AI"}
                    </button>
                </form>

                {/* Clear conversation */}
                {messages.length > 0 && (
                    <button
                        type="button"
                        className="clear-chat-button"
                        onClick={clearConversation}
                        disabled={loading}
                    >
                        Clear conversation
                    </button>
                )}

                <p className="disclaimer">
                    AI-generated answers can be inaccurate.
                    Check the cited policy documents or contact
                    HR for confirmation.
                </p>
            </main>
        </div>
    );
}

export default App;
