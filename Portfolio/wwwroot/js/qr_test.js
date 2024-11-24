document.addEventListener("DOMContentLoaded", function() {
    const uriElement = document.getElementById("qrCodeData");

    if (uriElement) {
        const uri = uriElement.getAttribute('data-url');
        new QRCode(document.getElementById("qrCode"), {
            text: uri,
            width: 150,
            height: 150
        });
    }
});