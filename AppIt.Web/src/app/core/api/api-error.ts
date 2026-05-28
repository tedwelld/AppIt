export function extractApiErrorMessage(error: unknown, fallback: string): string {
    if (!error) {
        return fallback;
    }

    if (typeof error === 'string') {
        return error;
    }

    const problem = error as {
        error?: {
            detail?: string;
            title?: string;
            message?: string;
            errors?: Record<string, string[] | string>;
        };
        message?: string;
    };

    const details = problem?.error?.errors;
    if (details && typeof details === 'object') {
        const messages = Object.entries(details)
            .flatMap(([key, value]) => {
                const items = Array.isArray(value) ? value : [value];
                return items
                    .filter((item): item is string => typeof item === 'string' && item.trim().length > 0)
                    .map((item) => `${key}: ${item}`);
            })
            .filter((item) => item.trim().length > 0);

        if (messages.length > 0) {
            return messages.join(' | ');
        }
    }

    const message = problem?.error?.detail
        ?? problem?.error?.title
        ?? problem?.error?.message
        ?? problem?.message;

    if (message) {
        return message;
    }

    if (problem?.error && typeof problem.error === 'object') {
        return summarizeObject(problem.error, fallback);
    }

    return typeof error === 'object' ? summarizeObject(error, fallback) : fallback;
}

function summarizeObject(value: unknown, fallback: string): string {
    if (!value || typeof value !== 'object') {
        return fallback;
    }

    const entries = Object.entries(value as Record<string, unknown>)
        .filter(([, item]) => item !== null && item !== undefined && typeof item !== 'object')
        .map(([key, item]) => `${key}: ${String(item)}`);

    return entries.length ? entries.join(' | ') : fallback;
}
