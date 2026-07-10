/**
 * workflow.model.ts — Domain interfaces for Warewolf workflow API contracts.
 *
 * These types mirror the JSON structures the wwexecution Function App
 * returns and accepts.  Keeping them in a dedicated model file makes
 * them easy to share across components and services without introducing
 * circular dependencies.
 */

// ── Request ────────────────────────────────────────────────────────────────────

/**
 * Generic key/value bag sent as the JSON body of a workflow POST request.
 * The Function App maps each key to a Warewolf scalar variable.
 *
 * Example: { "Name": "Alice", "Age": 30 }
 */
export type WorkflowInputs = Record<string, unknown>;

// ── Response ───────────────────────────────────────────────────────────────────

/**
 * Top-level response envelope returned by the wwexecution Function App.
 *
 * Warewolf serialises workflow output variables into the `outputs` map and
 * reports execution state in `status`.
 */
export interface WorkflowResult {
  /** Whether the workflow executed without errors: "Success" | "Failure" */
  status:  'Success' | 'Failure' | string;

  /** Workflow output variables keyed by Warewolf variable name. */
  outputs: Record<string, unknown>;

  /** Human-readable message, populated on failures. */
  message?: string;

  /** Correlation ID for tracing a specific execution in Application Insights. */
  executionId?: string;
}

// ── Auth context ───────────────────────────────────────────────────────────────

/**
 * Identifies which authentication path was used for a call.
 * Used by the demo UI to label each result panel.
 */
export type CallContext = 'public' | 'secure' | 'services' | 'client-credentials';
