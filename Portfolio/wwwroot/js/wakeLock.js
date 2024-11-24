let wakeLock = null;

// Expose the functions to the window object so Blazor can call them
window.requestWakeLock = async function () {
    try {
        wakeLock = await navigator.wakeLock.request('screen');
        wakeLock.addEventListener('release', () => {
            console.log('Wake lock was released');
        });
        console.log('Wake lock is active');
    } catch (err) {
        console.error(`${err.name}, ${err.message}`);
    }
};

window.releaseWakeLock = async function () {
    if (wakeLock !== null) {
        await wakeLock.release().then(() => {
            wakeLock = null;
            console.log('Wake lock released manually');
        });
    }
};

console.log("wakeLock.js loaded");

// Add event listener for when the page is fully loaded
window.addEventListener('load', () => {
    console.log('wakeLock.js loaded or refreshed');
    // Correctly call the window's requestWakeLock function
    window.requestWakeLock();
});
