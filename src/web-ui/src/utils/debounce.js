/**
 * Creates a debounced version of the given function that delays its execution
 * until after a specified delay has elapsed since the last time it was invoked.
 *
 * @template TArgs
 * @param {(this: any, ...args: TArgs[]) => void} func - The function to debounce.
 * @param {number} delay - The delay in milliseconds.
 * @returns {(this: any, ...args: TArgs[]) => void} A debounced version of the input function.
 */
export function debounce(func, delay) {
  /** @type {ReturnType<typeof setTimeout>} */
  let debounceTimeout;

  return function (...args) {
    const context = this;
    clearTimeout(debounceTimeout);
    debounceTimeout = setTimeout(() => func.apply(context, args), delay);
  };
}
