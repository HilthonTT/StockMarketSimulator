export function debounce(func, delay) {
  let debounceTimeout;
  return function (...args) {
    const context = this;
    clearTimeout(debounceTimeout);
    debounceTimeout = setTimeout(() => func.apply(context, args), delay);
  };
}
