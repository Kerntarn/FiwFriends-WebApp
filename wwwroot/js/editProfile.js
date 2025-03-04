async function updateUser() {
        const userData = {
            Username: document.getElementById("username").value,
            FirstName: document.getElementById("firstName").value,
            LastName: document.getElementById("lastName").value,
            Contact: document.getElementById("contact").value,
            Password: document.getElementById("password").value
        };
    
        try {
            const response = await fetch("/user/edit", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(userData)
            });
    
            const result = await response.json();
    
            if (response.ok) {
                alert("Profile updated successfully!");
                window.location.href = "/User/Edit"; // ไปหน้า Profile หลังจากอัปเดตสำเร็จ
            } else {
                alert(result.error || "Failed to update user information.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("An error occurred. Please try again.");
        }
    }
    