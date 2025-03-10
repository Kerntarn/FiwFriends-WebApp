async function changePassword() {
        const formData = new FormData();
    
        // formData.append("Username", "");
        // formData.append("FirstName", "");
        // formData.append("LastName", "");
        // formData.append("Bio", "");
        // formData.append("Contact", "");
        formData.append("OldPassword", document.getElementById("oldPassword")?.value);
        formData.append("NewPassword", document.getElementById("newPassword")?.value);
        formData.append("ConfirmNewPassword", document.getElementById("confirmPassword")?.value);
    
        for (let pair of formData.entries()) {
                console.log(pair[0], pair[1]);
        }

        try {
            const response = await fetch("/user/edit", {
                method: "POST",
                body: formData,
            });
    
            const result = await response.json();
    
            if (response.ok) {
                alert("Profile updated successfully!");
                window.location.href = "/User/Edit";
            } else {
                alert(result.error || "Failed to update user information.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("An error occurred. Please try again.");
        }
    }
    