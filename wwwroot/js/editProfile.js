async function updateUser() {
    const formData = new FormData();

    formData.append("Username", document.getElementById("username")?.value || "");
    formData.append("FirstName", document.getElementById("firstName")?.value || "");
    formData.append("LastName", document.getElementById("lastName")?.value || "");
    formData.append("Bio", document.getElementById("bio")?.value || "");
    formData.append("Contact", document.getElementById("contact")?.value || "");
    formData.append("NewPassword", document.getElementById("password")?.value || "");
    formData.append("ConfirmPassword", document.getElementById("confirmPassword")?.value || "");

    const fileInput = document.getElementById("profilePicInput");
    if (fileInput?.files.length > 0) {
        formData.append("ProfilePic", fileInput.files[0]);
    }

    // ✅ Debug: ตรวจสอบว่ามีค่าถูกต้องหรือไม่
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
