import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs))
}




/**
 * Recursively converts the keys of an object from snake_case to camelCase.
 * @param obj - The object to process.
 * @returns A new object with camelCase keys.
 */
export function convertKeysToCamelCase(obj: any): any {
    if (Array.isArray(obj)) {
        return obj.map(convertKeysToCamelCase);
    } else if (obj && typeof obj === "object" && obj.constructor === Object) {
        return Object.entries(obj).reduce((acc, [key, value]) => {
            const camelCaseKey = toCamelCase(key);
            acc[camelCaseKey] = convertKeysToCamelCase(value);
            return acc;
        }, {} as Record<string, any>);
    }
    return obj;
}

/**
 * Converts a snake_case string to camelCase.
 * @param snakeCaseString - The snake_case string to convert.
 * @returns The converted camelCase string.
 */
function toCamelCase(snakeCaseString: string): string {
    return snakeCaseString
        .toLowerCase()
        .replace(/_([a-z])/g, (_, letter: string) => letter.toUpperCase());
}