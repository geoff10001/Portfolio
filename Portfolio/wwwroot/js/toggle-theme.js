const themeToggleIcon = document.getElementById("theme-toggle");
const body = document.body;

// Check and apply saved theme on page load
const savedTheme = localStorage.getItem("theme") || "lightmode";
body.classList.add(savedTheme);

// Set the initial icon based on the theme
themeToggleIcon.classList.add(savedTheme === "darkmode" ? "oi-sun" : "oi-moon");

// Toggle theme on icon click
themeToggleIcon.addEventListener("click", () => {
    const isDarkMode = body.classList.contains("darkmode");

    // Toggle theme classes
    body.classList.toggle("darkmode", !isDarkMode);
    body.classList.toggle("lightmode", isDarkMode);

    // Update the icon class
    themeToggleIcon.classList.toggle("oi-moon", isDarkMode);
    themeToggleIcon.classList.toggle("oi-sun", !isDarkMode);

    // Save the user's preference
    localStorage.setItem("theme", isDarkMode ? "lightmode" : "darkmode");
});

