document.getElementById("loginForm").addEventListener("submit", async function (e) {
        e.preventDefault(); // ป้องกันหน้ารีเฟรช
    
        const formData = new FormData(this);
        const loginData = {
            Username: formData.get("Username"),
            Password: formData.get("Password")
        };
    
        try {
            const response = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(loginData)
            });
    
            const data = await response.json();
    
            if (response.ok) {
                alert("Login Successful!");
                window.location.href = "/Home"; // ไปหน้า Home
            } else {
                document.getElementById("errorMessage").innerText = data.message;
            }
        } catch (error) {
            console.error("Error:", error);
        }
    });