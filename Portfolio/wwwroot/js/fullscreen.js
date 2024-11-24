async function toggleFullScreen() {
    try {
        var iframe = document.querySelector('iframe');

        if (!iframe) {
            console.error("Iframe not found in DOM");
            return;
        }

        if (!document.fullscreenElement) {
            if (iframe.requestFullscreen) {
                await iframe.requestFullscreen();
            } else if (iframe.mozRequestFullScreen) {
                await iframe.mozRequestFullScreen();
            } else if (iframe.webkitRequestFullscreen) {
                await iframe.webkitRequestFullscreen();
            } else if (iframe.msRequestFullscreen) {
                await iframe.msRequestFullscreen();
            }
        } else {
            if (document.exitFullscreen) {
                await document.exitFullscreen();
            } else if (document.mozCancelFullScreen) {
                await document.mozCancelFullScreen();
            } else if (document.webkitExitFullscreen) {
                await document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                await document.msExitFullscreen();
            }
        }
    } catch (error) {
        console.error("Error toggling fullscreen:", error);
    }
}

